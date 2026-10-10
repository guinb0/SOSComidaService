using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Mappers;
using SOSComida.Models;
using SOSComida.Seguranca;
using SOSComida.Services;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/campanhas")]
public class CampanhasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CampanhasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/campanhas
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var campanhas = await _context.Campanhas
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => c.ToDto()).ToList();
        return Ok(new { data = campanhasDto, total = campanhasDto.Count });
    }

    // GET: api/campanhas/moderacao - Campanhas para moderação, filtradas pelas regiões do moderador
    [HttpGet("moderacao")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> GetParaModeracao()
    {
        var query = await FiltrarPorRegiaoDoModeradorAsync(_context.Campanhas);
        if (query == null)
            return Ok(new List<CampanhaDto>());

        var campanhas = await query
            .Include(c => c.Regiao)
            .OrderByDescending(c => c.Status == StatusCampanhas.Pendente) // Pendentes primeiro
            .ThenByDescending(c => c.DataCriacao)
            .ToListAsync();

        return Ok(campanhas.Select(c => c.ToDto()).ToList());
    }

    // GET: api/campanhas/pendentes - Campanhas pendentes de aprovação
    [HttpGet("pendentes")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> GetPendentes()
    {
        var query = await FiltrarPorRegiaoDoModeradorAsync(_context.Campanhas.Where(c => c.Status == StatusCampanhas.Pendente));
        if (query == null)
            return Ok(new List<CampanhaDto>());

        var campanhas = await query
            .Include(c => c.Regiao)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();

        return Ok(campanhas.Select(c => c.ToDto()).ToList());
    }

    // GET: api/campanhas/principais
    [HttpGet("principais")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPrincipais()
    {
        var campanhas = await _context.Campanhas
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .Take(3)
            .ToListAsync();

        return Ok(new { data = campanhas.Select(c => c.ToDto()).ToList() });
    }

    // GET: api/campanhas/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        return Ok(campanha.ToDto());
    }

    // GET: api/campanhas/usuario/5
    [HttpGet("usuario/{usuarioId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUsuario(int usuarioId)
    {
        var campanhas = await _context.Campanhas
            .Where(c => c.UsuarioId == usuarioId && c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => c.ToDto()).ToList();
        return Ok(new { data = campanhasDto, total = campanhasDto.Count });
    }

    // POST: api/campanhas
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampanhaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return BadRequest(new { message = "Título é obrigatório" });

        if (string.IsNullOrWhiteSpace(dto.Descricao))
            return BadRequest(new { message = "Descrição é obrigatória" });

        if (dto.MetaArrecadacao <= 0)
            return BadRequest(new { message = "Meta de arrecadação deve ser maior que zero" });

        if (dto.DataFim < dto.DataInicio)
            return BadRequest(new { message = "A data de término deve ser depois do início" });

        var campanha = new Campanha
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            ImagemUrl = dto.ImagemUrl ?? dto.Imagens?.FirstOrDefault()
                        ?? "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800",
            Imagens = dto.Imagens is { Count: > 0 } ? JsonSerializer.Serialize(dto.Imagens) : null,
            Localizacao = dto.Localizacao ?? "Brasil",
            MetaArrecadacao = dto.MetaArrecadacao,
            ValorArrecadado = 0,
            DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc),
            DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),
            Status = StatusCampanhas.Pendente, // Precisa aprovação de moderador
            Ativa = false, // Só fica ativa após aprovação
            DataCriacao = DateTime.UtcNow,
            UsuarioId = User.Id(),
            RegiaoId = dto.RegiaoId
        };

        _context.Campanhas.Add(campanha);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = campanha.Id }, campanha.ToDto());
    }

    // PUT: api/campanhas/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampanhaDto dto)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!PermissoesCampanha.PodeResponderPor(campanha, User))
            return Forbid();

        if (dto.MetaArrecadacao <= 0)
            return BadRequest(new { message = "Meta de arrecadação deve ser maior que zero" });

        if (dto.Status != null && dto.Status != campanha.Status)
        {
            var erro = ValidarMudancaDeStatus(campanha, dto.Status);
            if (erro != null)
                return StatusCode(erro.Value.Codigo, new { message = erro.Value.Mensagem });

            campanha.Status = dto.Status;
            campanha.Ativa = dto.Status == StatusCampanhas.Ativa;
        }

        campanha.Titulo = dto.Titulo;
        campanha.Descricao = dto.Descricao;
        campanha.ImagemUrl = dto.ImagemUrl ?? campanha.ImagemUrl;
        campanha.Localizacao = dto.Localizacao ?? campanha.Localizacao;
        campanha.MetaArrecadacao = dto.MetaArrecadacao;
        campanha.DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc);
        campanha.DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc);
        campanha.DataAtualizacao = DateTime.UtcNow;

        if (User.EhModerador() && campanha.UsuarioId != User.Id())
            _context.Registrar(User.Id(), AcoesAuditadas.CampanhaEditada, ItensAuditados.Campanha, id,
                $"Editou a campanha \"{campanha.Titulo}\"");

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/campanhas/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!PermissoesCampanha.PodeResponderPor(campanha, User))
            return Forbid();

        campanha.Ativa = false;
        campanha.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/campanhas/5/doar - Formato antigo (valor puro no corpo); prefira POST /doacoes
    [HttpPost("{id}/doar")]
    public async Task<IActionResult> Doar(int id, [FromBody] decimal valor)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!campanha.Ativa)
            return BadRequest(new { message = "Esta campanha não está recebendo doações" });

        if (valor <= 0)
            return BadRequest(new { message = "Valor da doação deve ser maior que zero" });

        await DoacoesController.RegistrarAsync(_context, campanha, User.Id(), valor, metodo: null);
        await _context.SaveChangesAsync();

        var percentual = campanha.MetaArrecadacao > 0
            ? (int)((campanha.ValorArrecadado / campanha.MetaArrecadacao) * 100)
            : 0;

        return Ok(new
        {
            message = "Doação realizada com sucesso!",
            valorArrecadado = campanha.ValorArrecadado,
            percentual = percentual
        });
    }

    // POST: api/campanhas/5/aprovar
    [HttpPost("{id}/aprovar")]
    [Authorize(Policy = Politicas.Moderacao)]
    public Task<IActionResult> Aprovar(int id) =>
        MudarStatusAsync(id, StatusCampanhas.Ativa, ativa: true, "Campanha aprovada com sucesso!", AcoesAuditadas.CampanhaAprovada);

    // POST: api/campanhas/5/pausar
    [HttpPost("{id}/pausar")]
    [Authorize(Policy = Politicas.Moderacao)]
    public Task<IActionResult> Pausar(int id) =>
        MudarStatusAsync(id, StatusCampanhas.Pausada, ativa: false, "Campanha pausada com sucesso!", AcoesAuditadas.CampanhaPausada);

    // POST: api/campanhas/5/rejeitar
    [HttpPost("{id}/rejeitar")]
    [Authorize(Policy = Politicas.Moderacao)]
    public Task<IActionResult> Rejeitar(int id) =>
        MudarStatusAsync(id, StatusCampanhas.Rejeitada, ativa: false, "Campanha rejeitada.", AcoesAuditadas.CampanhaRejeitada);

    // POST: api/campanhas/5/finalizar - Encerra a campanha (responsável ou moderação)
    [HttpPost("{id}/finalizar")]
    public async Task<IActionResult> Finalizar(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!PermissoesCampanha.PodeResponderPor(campanha, User))
            return Forbid();

        campanha.Status = StatusCampanhas.Finalizada;
        campanha.Ativa = false;
        campanha.DataAtualizacao = DateTime.UtcNow;
        if (User.EhModerador())
            _context.Registrar(User.Id(), AcoesAuditadas.CampanhaFinalizada, ItensAuditados.Campanha, id,
                $"Finalizou a campanha \"{campanha.Titulo}\"");
        await _context.SaveChangesAsync();

        return Ok(new { message = "Campanha finalizada com sucesso!" });
    }

    // POST: api/campanhas/5/delegar - Delegar campanha para uma instituição
    [HttpPost("{id}/delegar")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> DelegarCampanha(int id, [FromBody] DelegarCampanhaRequest request)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        var instituicao = await _context.Usuarios.FindAsync(request.InstituicaoId);
        if (instituicao == null)
            return NotFound(new { message = "Instituição não encontrada" });

        if (instituicao.Tipo != TiposUsuario.Instituicao)
            return BadRequest(new { message = "O usuário selecionado não é uma instituição" });

        // Atualizar campanha com delegação pendente
        campanha.InstituicaoId = request.InstituicaoId;
        campanha.StatusDelegacao = StatusDelegacoes.Pendente;
        campanha.Status = StatusCampanhas.Delegada;
        campanha.DataDelegacao = DateTime.UtcNow;
        campanha.DataAtualizacao = DateTime.UtcNow;

        _context.Notificacoes.Add(new Notificacao
        {
            UsuarioId = request.InstituicaoId,
            Tipo = TiposNotificacao.DelegacaoCampanha,
            Titulo = "Nova campanha para você gerenciar!",
            Mensagem = $"A campanha '{campanha.Titulo}' foi delegada para sua instituição. Você pode aceitar ou recusar.",
            CampanhaId = id,
            StatusDelegacao = StatusDelegacoes.Pendente
        });
        _context.Registrar(User.Id(), AcoesAuditadas.CampanhaDelegada, ItensAuditados.Campanha, id,
            $"Delegou a campanha \"{campanha.Titulo}\" para {instituicao.Nome}");
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Campanha delegada para {instituicao.Nome}. Aguardando aceitação.",
            instituicao = instituicao.Nome
        });
    }

    // GET: api/campanhas/instituicoes - Listar instituições disponíveis para delegação
    [HttpGet("instituicoes")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> GetInstituicoes()
    {
        var instituicoes = await _context.Usuarios
            .Where(u => u.Tipo == TiposUsuario.Instituicao)
            .OrderBy(u => u.Nome)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Cidade,
                u.Endereco
            })
            .ToListAsync();

        return Ok(new { data = instituicoes, total = instituicoes.Count });
    }

    // POST: api/campanhas/5/participantes - Inscrever o usuário logado como voluntário
    [HttpPost("{id}/participantes")]
    public async Task<IActionResult> AdicionarParticipante(int id, [FromBody] AdicionarParticipanteRequest request)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        var usuarioId = User.Id();
        var jaParticipa = await _context.ParticipantesCampanha
            .AnyAsync(p => p.CampanhaId == id && p.UsuarioId == usuarioId && p.Status == StatusParticipantes.Ativo);

        if (jaParticipa)
            return BadRequest(new { message = "Você já está participando desta campanha" });

        var participante = new ParticipanteCampanha
        {
            CampanhaId = id,
            UsuarioId = usuarioId,
            Tipo = request.Tipo is "doador" ? "doador" : "voluntario",
            Status = StatusParticipantes.Ativo,
            DataEntrada = DateTime.UtcNow
        };

        _context.ParticipantesCampanha.Add(participante);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Inscrição realizada com sucesso!",
            participanteId = participante.Id
        });
    }

    // GET: api/campanhas/5/participantes - Listar participantes (com contato: só responsável/moderação)
    [HttpGet("{id}/participantes")]
    public async Task<IActionResult> GetParticipantes(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!PermissoesCampanha.PodeResponderPor(campanha, User))
            return Forbid();

        var participantes = await _context.ParticipantesCampanha
            .Include(p => p.Usuario)
            .Where(p => p.CampanhaId == id)
            .Select(p => new
            {
                p.Id,
                p.UsuarioId,
                NomeUsuario = p.Usuario.Nome,
                EmailUsuario = p.Usuario.Email,
                TelefoneUsuario = p.Usuario.Telefone,
                p.Tipo,
                p.Status,
                DataInscricao = p.DataEntrada,
                p.MotivoSaida
            })
            .ToListAsync();

        return Ok(new { data = participantes, total = participantes.Count });
    }

    // POST: api/campanhas/5/participantes/1/advertencia - Enviar advertência a um participante
    [HttpPost("{campanhaId}/participantes/{participanteId}/advertencia")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> EnviarAdvertencia(int campanhaId, int participanteId, [FromBody] AdvertenciaParticipanteRequest request)
    {
        var participante = await _context.ParticipantesCampanha
            .FirstOrDefaultAsync(p => p.Id == participanteId && p.CampanhaId == campanhaId);

        if (participante == null)
            return NotFound(new { message = "Participante não encontrado" });

        var aviso = new AvisoCampanha
        {
            CampanhaId = campanhaId,
            DestinatarioId = participante.UsuarioId,
            ModeradorId = User.Id(),
            Titulo = "Advertência",
            Mensagem = request.Mensagem,
            Tipo = TiposAviso.Advertencia,
            DataEnvio = DateTime.UtcNow,
            Lido = false
        };

        _context.AvisosCampanha.Add(aviso);
        _context.Registrar(User.Id(), AcoesAuditadas.Advertencia, ItensAuditados.Usuario, participante.UsuarioId,
            "Enviou advertência a um participante", request.Mensagem);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Advertência enviada com sucesso!",
            avisoId = aviso.Id
        });
    }

    // POST: api/campanhas/5/participantes/1/remover - Remover participante (pelo id da participação)
    [HttpPost("{campanhaId}/participantes/{participanteId}/remover")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> RemoverParticipanteCampanha(int campanhaId, int participanteId, [FromBody] MotivoRequest request)
    {
        var participante = await _context.ParticipantesCampanha
            .Include(p => p.Campanha)
            .FirstOrDefaultAsync(p => p.Id == participanteId && p.CampanhaId == campanhaId);

        if (participante == null)
            return NotFound(new { message = "Participante não encontrado" });

        var aviso = Remover(participante, request.Motivo);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Participante removido com sucesso!",
            avisoId = aviso.Id
        });
    }

    // DELETE: api/campanhas/5/participantes/{usuarioId} - Remover participante (pelo id do usuário)
    [HttpDelete("{campanhaId}/participantes/{usuarioId}")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> RemoverParticipantePorUsuario(int campanhaId, int usuarioId, [FromBody] MotivoRequest request)
    {
        var participante = await _context.ParticipantesCampanha
            .Include(p => p.Campanha)
            .FirstOrDefaultAsync(p => p.CampanhaId == campanhaId && p.UsuarioId == usuarioId && p.Status == StatusParticipantes.Ativo);

        if (participante == null)
            return NotFound(new { message = "Participante não encontrado" });

        Remover(participante, request.Motivo);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Participante removido com sucesso" });
    }

    // GET: api/campanhas/5/chat - Mensagens do chat (participantes, responsável e moderação)
    [HttpGet("{id}/chat")]
    public async Task<IActionResult> GetMensagensChat(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!await PodeUsarChatAsync(campanha))
            return Forbid();

        var mensagens = await _context.MensagensChat
            .Where(m => m.CampanhaId == id && !m.Deletada)
            .OrderBy(m => m.DataEnvio)
            .Select(m => new MensagemChatDto
            {
                Id = m.Id,
                CampanhaId = m.CampanhaId,
                UsuarioId = m.UsuarioId,
                NomeUsuario = m.Usuario.Nome,
                Conteudo = m.Conteudo,
                DataEnvio = m.DataEnvio,
                DataEdicao = m.DataEdicao,
                Fixada = m.Fixada,
                FixadaPorNome = m.FixadaPor != null ? m.FixadaPor.Nome : null
            })
            .ToListAsync();

        return Ok(new { data = mensagens, total = mensagens.Count });
    }

    // POST: api/campanhas/5/chat - Enviar mensagem no chat
    [HttpPost("{id}/chat")]
    public async Task<IActionResult> EnviarMensagemChat(int id, [FromBody] EnviarMensagemRequest request)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!await PodeUsarChatAsync(campanha))
            return StatusCode(403, new { message = "Você precisa ser voluntário desta campanha para enviar mensagens" });

        var mensagem = new MensagemChat
        {
            CampanhaId = id,
            UsuarioId = User.Id(),
            Conteudo = request.Conteudo.Trim(),
            DataEnvio = DateTime.UtcNow,
            Fixada = false,
            Deletada = false
        };

        _context.MensagensChat.Add(mensagem);
        await _context.SaveChangesAsync();

        return Ok(new MensagemChatDto
        {
            Id = mensagem.Id,
            CampanhaId = mensagem.CampanhaId,
            UsuarioId = mensagem.UsuarioId,
            NomeUsuario = User.Identity?.Name ?? "",
            Conteudo = mensagem.Conteudo,
            DataEnvio = mensagem.DataEnvio,
            Fixada = mensagem.Fixada
        });
    }

    // DELETE: api/campanhas/5/chat/10
    [HttpDelete("{campanhaId}/chat/{mensagemId}")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> DeletarMensagem(int campanhaId, int mensagemId)
    {
        var mensagem = await _context.MensagensChat
            .FirstOrDefaultAsync(m => m.Id == mensagemId && m.CampanhaId == campanhaId);

        if (mensagem == null)
            return NotFound(new { message = "Mensagem não encontrada" });

        mensagem.Deletada = true;
        _context.Registrar(User.Id(), AcoesAuditadas.MensagemApagada, ItensAuditados.Mensagem, mensagemId,
            "Apagou uma mensagem do chat", mensagem.Conteudo);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Mensagem deletada com sucesso" });
    }

    // PUT: api/campanhas/5/chat/10/fixar - Fixa ou desafixa
    [HttpPut("{campanhaId}/chat/{mensagemId}/fixar")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> FixarMensagem(int campanhaId, int mensagemId)
    {
        var mensagem = await _context.MensagensChat
            .FirstOrDefaultAsync(m => m.Id == mensagemId && m.CampanhaId == campanhaId);

        if (mensagem == null)
            return NotFound(new { message = "Mensagem não encontrada" });

        mensagem.Fixada = !mensagem.Fixada;
        mensagem.FixadaPorId = mensagem.Fixada ? User.Id() : null;
        await _context.SaveChangesAsync();

        return Ok(new { message = mensagem.Fixada ? "Mensagem fixada" : "Mensagem desafixada" });
    }

    /// <summary>
    /// Restringe às regiões atribuídas ao moderador logado. Admin vê tudo.
    /// Devolve null quando o moderador não tem região nenhuma (não vê nada).
    /// </summary>
    private async Task<IQueryable<Campanha>?> FiltrarPorRegiaoDoModeradorAsync(IQueryable<Campanha> query)
    {
        if (User.EhAdmin())
            return query;

        var moderadorId = User.Id();
        var regioes = await _context.ModeradorRegioes
            .Where(mr => mr.ModeradorId == moderadorId && mr.Ativo)
            .Select(mr => mr.RegiaoId)
            .ToListAsync();

        if (regioes.Count == 0)
            return null;

        return query.Where(c => c.RegiaoId.HasValue && regioes.Contains(c.RegiaoId.Value));
    }

    private static readonly string[] StatusEditaveis =
        { StatusCampanhas.Ativa, StatusCampanhas.Pausada, StatusCampanhas.Finalizada };

    /// <summary>
    /// O responsável pode pausar ou encerrar a própria campanha; colocar no ar
    /// (ativa) é decisão da moderação — senão a edição pularia a aprovação.
    /// </summary>
    private (int Codigo, string Mensagem)? ValidarMudancaDeStatus(Campanha campanha, string novo)
    {
        if (!StatusEditaveis.Contains(novo))
            return (400, "Status inválido. Use ativa, pausada ou finalizada.");

        if (campanha.Status == StatusCampanhas.Finalizada && !User.EhModerador())
            return (403, "Campanha finalizada não pode ser reaberta pelo responsável.");

        if (novo == StatusCampanhas.Ativa && !User.EhModerador())
            return (403, "Só a moderação pode ativar uma campanha.");

        return null;
    }

    private async Task<IActionResult> MudarStatusAsync(int id, string status, bool ativa, string mensagem, string acao)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        campanha.Status = status;
        campanha.Ativa = ativa;
        campanha.DataAtualizacao = DateTime.UtcNow;
        _context.Registrar(User.Id(), acao, ItensAuditados.Campanha, id, $"{mensagem.TrimEnd('!', '.')}: \"{campanha.Titulo}\"");

        await _context.SaveChangesAsync();

        return Ok(new { message = mensagem });
    }

    private async Task<bool> PodeUsarChatAsync(Campanha campanha)
    {
        if (PermissoesCampanha.PodeResponderPor(campanha, User))
            return true;

        var usuarioId = User.Id();
        return await _context.ParticipantesCampanha
            .AnyAsync(p => p.CampanhaId == campanha.Id && p.UsuarioId == usuarioId && p.Status == StatusParticipantes.Ativo);
    }

    private AvisoCampanha Remover(ParticipanteCampanha participante, string motivo)
    {
        participante.Status = StatusParticipantes.Removido;
        participante.DataSaida = DateTime.UtcNow;
        participante.MotivoSaida = motivo;
        participante.RemovidoPorId = User.Id();

        var aviso = new AvisoCampanha
        {
            CampanhaId = participante.CampanhaId,
            DestinatarioId = participante.UsuarioId,
            ModeradorId = User.Id(),
            Titulo = "Remoção da Campanha",
            Mensagem = $"Você foi removido da campanha \"{participante.Campanha.Titulo}\". Motivo: {motivo}",
            Tipo = TiposAviso.Urgente,
            DataEnvio = DateTime.UtcNow,
            Lido = false
        };
        _context.AvisosCampanha.Add(aviso);
        _context.Registrar(User.Id(), AcoesAuditadas.ParticipanteRemovido, ItensAuditados.Usuario, participante.UsuarioId,
            $"Removeu um participante da campanha \"{participante.Campanha.Titulo}\"", motivo);
        return aviso;
    }
}

public class AdicionarParticipanteRequest
{
    public string? Tipo { get; set; }
}

public class AdvertenciaParticipanteRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Escreva a mensagem da advertência")]
    public string Mensagem { get; set; } = string.Empty;
}

public class MotivoRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o motivo")]
    public string Motivo { get; set; } = string.Empty;
}

public class DelegarCampanhaRequest
{
    public int InstituicaoId { get; set; }
}
