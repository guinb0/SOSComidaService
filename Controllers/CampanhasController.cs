using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Models;

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
    public async Task<ActionResult<IEnumerable<CampanhaDto>>> GetAll()
    {
        var campanhas = await _context.Campanhas
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa
        }).ToList();
        
        return Ok(new { data = campanhasDto, total = campanhasDto.Count });
    }

    // GET: api/campanhas/moderacao - Retorna campanhas para moderação (filtradas por região do moderador)
    [HttpGet("moderacao")]
    public async Task<ActionResult<IEnumerable<CampanhaDto>>> GetParaModeracao([FromQuery] int? moderadorId)
    {
        IQueryable<Campanha> query = _context.Campanhas;

        // Se foi passado moderadorId, filtrar pelas regiões do moderador
        if (moderadorId.HasValue)
        {
            var moderador = await _context.Usuarios.FindAsync(moderadorId.Value);
            
            // Admin vê tudo
            if (moderador?.Tipo != "admin")
            {
                var regioesDoModerador = await _context.ModeradorRegioes
                    .Where(mr => mr.ModeradorId == moderadorId.Value && mr.Ativo)
                    .Select(mr => mr.RegiaoId)
                    .ToListAsync();

                // Se o moderador tem regiões atribuídas, filtrar
                if (regioesDoModerador.Any())
                {
                    query = query.Where(c => c.RegiaoId.HasValue && regioesDoModerador.Contains(c.RegiaoId.Value));
                }
                else
                {
                    // Moderador sem regiões não vê nada
                    return Ok(new List<CampanhaDto>());
                }
            }
        }

        var campanhas = await query
            .Include(c => c.Regiao)
            .OrderByDescending(c => c.Status == "pendente") // Pendentes primeiro
            .ThenByDescending(c => c.DataCriacao)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa,
            RegiaoId = c.RegiaoId,
            RegiaoNome = c.Regiao?.Nome
        }).ToList();
        
        return Ok(campanhasDto);
    }

    // GET: api/campanhas/pendentes - Campanhas pendentes de aprovação
    [HttpGet("pendentes")]
    public async Task<ActionResult<IEnumerable<CampanhaDto>>> GetPendentes([FromQuery] int? moderadorId)
    {
        IQueryable<Campanha> query = _context.Campanhas
            .Where(c => c.Status == "pendente");

        // Se foi passado moderadorId, filtrar pelas regiões do moderador
        if (moderadorId.HasValue)
        {
            var regioesDoModerador = await _context.ModeradorRegioes
                .Where(mr => mr.ModeradorId == moderadorId.Value && mr.Ativo)
                .Select(mr => mr.RegiaoId)
                .ToListAsync();

            // Se o moderador tem regiões atribuídas, filtrar
            if (regioesDoModerador.Any())
            {
                query = query.Where(c => c.RegiaoId.HasValue && regioesDoModerador.Contains(c.RegiaoId.Value));
            }
            // Se não tem regiões, verifica se é admin (admin vê tudo)
            else
            {
                var moderador = await _context.Usuarios.FindAsync(moderadorId.Value);
                if (moderador?.Tipo != "admin")
                {
                    // Moderador sem regiões não vê nada
                    return Ok(new List<CampanhaDto>());
                }
            }
        }

        var campanhas = await query
            .Include(c => c.Regiao)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa,
            RegiaoId = c.RegiaoId,
            RegiaoNome = c.Regiao?.Nome
        }).ToList();
        
        return Ok(campanhasDto);
    }

    // GET: api/campanhas/principais
    [HttpGet("principais")]
    public async Task<ActionResult<IEnumerable<CampanhaDto>>> GetPrincipais()
    {
        var campanhas = await _context.Campanhas
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .Take(3)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa
        }).ToList();
        
        return Ok(new { data = campanhasDto });
    }

    // GET: api/campanhas/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CampanhaDto>> GetById(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        return Ok(new CampanhaDto
        {
            Id = campanha.Id,
            Titulo = campanha.Titulo,
            Descricao = campanha.Descricao,
            ImagemUrl = campanha.ImagemUrl,
            Localizacao = campanha.Localizacao,
            MetaArrecadacao = campanha.MetaArrecadacao,
            ValorArrecadado = campanha.ValorArrecadado,
            Progresso = campanha.MetaArrecadacao > 0 ? (int)((campanha.ValorArrecadado / campanha.MetaArrecadacao) * 100) : 0,
            DataInicio = campanha.DataInicio,
            DataFim = campanha.DataFim,
            Status = campanha.Status,
            Ativa = campanha.Ativa
        });
    }

    // GET: api/campanhas/usuario/5
    [HttpGet("usuario/{usuarioId}")]
    public async Task<ActionResult<IEnumerable<CampanhaDto>>> GetByUsuario(int usuarioId)
    {
        var campanhas = await _context.Campanhas
            .Where(c => c.UsuarioId == usuarioId && c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa
        }).ToList();
        
        return Ok(new { data = campanhasDto, total = campanhasDto.Count });
    }

    // POST: api/campanhas
    [HttpPost]
    public async Task<ActionResult<CampanhaDto>> Create([FromBody] CreateCampanhaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return BadRequest(new { message = "Título é obrigatório" });

        if (string.IsNullOrWhiteSpace(dto.Descricao))
            return BadRequest(new { message = "Descrição é obrigatória" });

        if (dto.MetaArrecadacao <= 0)
            return BadRequest(new { message = "Meta de arrecadação deve ser maior que zero" });

        var campanha = new Campanha
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            ImagemUrl = dto.ImagemUrl ?? "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800",
            Localizacao = dto.Localizacao ?? "Brasil",
            MetaArrecadacao = dto.MetaArrecadacao,
            ValorArrecadado = 0,
            DataInicio = DateTime.SpecifyKind(dto.DataInicio, DateTimeKind.Utc),
            DataFim = DateTime.SpecifyKind(dto.DataFim, DateTimeKind.Utc),
            Status = "pendente", // Precisa aprovação de moderador
            Ativa = false, // Só fica ativa após aprovação
            DataCriacao = DateTime.UtcNow,
            UsuarioId = null, // TODO: Pegar do token JWT quando implementar autenticação
            RegiaoId = dto.RegiaoId
        };

        _context.Campanhas.Add(campanha);
        await _context.SaveChangesAsync();

        var campanhaDto = new CampanhaDto
        {
            Id = campanha.Id,
            Titulo = campanha.Titulo,
            Descricao = campanha.Descricao,
            ImagemUrl = campanha.ImagemUrl,
            Localizacao = campanha.Localizacao,
            MetaArrecadacao = campanha.MetaArrecadacao,
            ValorArrecadado = campanha.ValorArrecadado,
            Progresso = 0,
            DataInicio = campanha.DataInicio,
            DataFim = campanha.DataFim,
            Status = campanha.Status,
            Ativa = campanha.Ativa
        };

        return CreatedAtAction(nameof(GetById), new { id = campanha.Id }, campanhaDto);
    }

    // PUT: api/campanhas/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampanhaDto dto)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        campanha.Titulo = dto.Titulo;
        campanha.Descricao = dto.Descricao;
        campanha.ImagemUrl = dto.ImagemUrl;
        campanha.MetaArrecadacao = dto.MetaArrecadacao;
        campanha.DataInicio = dto.DataInicio;
        campanha.DataFim = dto.DataFim;
        campanha.DataAtualizacao = DateTime.UtcNow;

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

        campanha.Ativa = false;
        campanha.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/campanhas/5/doar
    [HttpPost("{id}/doar")]
    public async Task<IActionResult> Doar(int id, [FromBody] decimal valor)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (valor <= 0)
            return BadRequest(new { message = "Valor da doação deve ser maior que zero" });

        campanha.ValorArrecadado += valor;
        campanha.DataAtualizacao = DateTime.UtcNow;

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
    public async Task<IActionResult> Aprovar(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        campanha.Status = "ativa";
        campanha.Ativa = true;
        campanha.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Campanha aprovada com sucesso!" });
    }

    // POST: api/campanhas/5/pausar
    [HttpPost("{id}/pausar")]
    public async Task<IActionResult> Pausar(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        campanha.Status = "pausada";
        campanha.Ativa = false;
        campanha.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Campanha pausada com sucesso!" });
    }

    // POST: api/campanhas/5/rejeitar
    [HttpPost("{id}/rejeitar")]
    public async Task<IActionResult> Rejeitar(int id)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        campanha.Status = "rejeitada";
        campanha.Ativa = false;
        campanha.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Campanha rejeitada." });
    }

    // POST: api/campanhas/5/delegar - Delegar campanha para uma instituição
    [HttpPost("{id}/delegar")]
    public async Task<IActionResult> DelegarCampanha(int id, [FromBody] DelegarCampanhaRequest request)
    {
        var campanha = await _context.Campanhas.FindAsync(id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        var instituicao = await _context.Usuarios.FindAsync(request.InstituicaoId);
        if (instituicao == null)
            return NotFound(new { message = "Instituição não encontrada" });

        if (instituicao.Tipo != "instituicao")
            return BadRequest(new { message = "O usuário selecionado não é uma instituição" });

        // Atualizar campanha com delegação pendente
        campanha.InstituicaoId = request.InstituicaoId;
        campanha.StatusDelegacao = "pendente";
        campanha.Status = "delegada";
        campanha.DataDelegacao = DateTime.UtcNow;
        campanha.DataAtualizacao = DateTime.UtcNow;

        // Criar notificação para a instituição
        var notificacao = new Notificacao
        {
            UsuarioId = request.InstituicaoId,
            Tipo = "delegacao_campanha",
            Titulo = "Nova campanha para você gerenciar!",
            Mensagem = $"A campanha '{campanha.Titulo}' foi delegada para sua instituição. Você pode aceitar ou recusar.",
            CampanhaId = id,
            StatusDelegacao = "pendente"
        };

        _context.Notificacoes.Add(notificacao);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = $"Campanha delegada para {instituicao.Nome}. Aguardando aceitação.",
            instituicao = instituicao.Nome
        });
    }

    // GET: api/campanhas/instituicoes - Listar instituições disponíveis para delegação
    [HttpGet("instituicoes")]
    public async Task<IActionResult> GetInstituicoes()
    {
        var instituicoes = await _context.Usuarios
            .Where(u => u.Tipo == "instituicao")
            .OrderBy(u => u.Nome)
            .Select(u => new {
                u.Id,
                u.Nome,
                u.Email,
                u.Cidade,
                u.Endereco
            })
            .ToListAsync();

        return Ok(new { data = instituicoes, total = instituicoes.Count });
    }

    // POST: api/campanhas/5/participantes - Inscrever como voluntário
    [HttpPost("{id}/participantes")]
    public async Task<IActionResult> AdicionarParticipante(int id, [FromBody] AdicionarParticipanteRequest request)
    {
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        // Verificar se já é participante
        var jaParticipa = await _context.ParticipantesCampanha
            .AnyAsync(p => p.CampanhaId == id && p.UsuarioId == request.UsuarioId && p.Status == "ativo");

        if (jaParticipa)
            return BadRequest(new { message = "Você já está participando desta campanha" });

        var participante = new ParticipanteCampanha
        {
            CampanhaId = id,
            UsuarioId = request.UsuarioId,
            Tipo = request.Tipo ?? "voluntario",
            Status = "ativo",
            DataEntrada = DateTime.UtcNow
        };

        _context.ParticipantesCampanha.Add(participante);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Inscrição realizada com sucesso!",
            participanteId = participante.Id
        });
    }

    // GET: api/campanhas/5/participantes - Listar participantes
    [HttpGet("{id}/participantes")]
    public async Task<IActionResult> GetParticipantes(int id)
    {
        var participantes = await _context.ParticipantesCampanha
            .Include(p => p.Usuario)
            .Where(p => p.CampanhaId == id)
            .Select(p => new {
                p.Id,
                p.UsuarioId,
                NomeUsuario = p.Usuario.Nome,
                EmailUsuario = p.Usuario.Email,
                TelefoneUsuario = p.Usuario.Telefone,
                p.Tipo,
                p.Status,
                DataInscricao = p.DataEntrada
            })
            .ToListAsync();

        return Ok(new { data = participantes, total = participantes.Count });
    }

    // POST: api/campanhas/5/participantes/1/advertencia - Enviar advertência a um participante
    [HttpPost("{campanhaId}/participantes/{participanteId}/advertencia")]
    public async Task<IActionResult> EnviarAdvertencia(int campanhaId, int participanteId, [FromBody] AdvertenciaParticipanteRequest request)
    {
        var participante = await _context.ParticipantesCampanha
            .Include(p => p.Usuario)
            .Include(p => p.Campanha)
            .FirstOrDefaultAsync(p => p.Id == participanteId && p.CampanhaId == campanhaId);

        if (participante == null)
            return NotFound(new { message = "Participante não encontrado" });

        var moderador = await _context.Usuarios.FindAsync(request.ModeradorId);
        if (moderador == null || (moderador.Tipo != "moderador" && moderador.Tipo != "admin"))
            return Unauthorized(new { message = "Apenas moderadores podem enviar advertências" });

        // Criar aviso para o usuário
        var aviso = new AvisoCampanha
        {
            CampanhaId = campanhaId,
            DestinatarioId = participante.UsuarioId,
            ModeradorId = request.ModeradorId,
            Titulo = "Advertência",
            Mensagem = request.Mensagem,
            Tipo = "advertencia",
            DataEnvio = DateTime.UtcNow,
            Lido = false
        };

        _context.AvisosCampanha.Add(aviso);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Advertência enviada com sucesso!",
            avisoId = aviso.Id
        });
    }

    // POST: api/campanhas/5/participantes/1/remover - Remover participante da campanha
    [HttpPost("{campanhaId}/participantes/{participanteId}/remover")]
    public async Task<IActionResult> RemoverParticipanteCampanha(int campanhaId, int participanteId, [FromBody] RemoverParticipanteCampanhaRequest request)
    {
        var participante = await _context.ParticipantesCampanha
            .Include(p => p.Usuario)
            .Include(p => p.Campanha)
            .FirstOrDefaultAsync(p => p.Id == participanteId && p.CampanhaId == campanhaId);

        if (participante == null)
            return NotFound(new { message = "Participante não encontrado" });

        var moderador = await _context.Usuarios.FindAsync(request.ModeradorId);
        if (moderador == null || (moderador.Tipo != "moderador" && moderador.Tipo != "admin"))
            return Unauthorized(new { message = "Apenas moderadores podem remover participantes" });

        // Atualizar status do participante
        participante.Status = "removido";
        participante.DataSaida = DateTime.UtcNow;
        participante.MotivoSaida = request.Motivo;
        participante.RemovidoPorId = request.ModeradorId;

        // Criar aviso para o usuário
        var aviso = new AvisoCampanha
        {
            CampanhaId = campanhaId,
            DestinatarioId = participante.UsuarioId,
            ModeradorId = request.ModeradorId,
            Titulo = "Remoção da Campanha",
            Mensagem = $"Você foi removido da campanha \"{participante.Campanha.Titulo}\". Motivo: {request.Motivo}",
            Tipo = "urgente",
            DataEnvio = DateTime.UtcNow,
            Lido = false
        };

        _context.AvisosCampanha.Add(aviso);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Participante removido com sucesso!",
            avisoId = aviso.Id
        });
    }

    // GET: api/campanhas/5/chat - Listar mensagens do chat
    [HttpGet("{id}/chat")]
    public async Task<IActionResult> GetMensagensChat(int id)
    {
        var mensagens = await _context.MensagensChat
            .Include(m => m.Usuario)
            .Where(m => m.CampanhaId == id && !m.Deletada)
            .OrderBy(m => m.DataEnvio)
            .Select(m => new {
                m.Id,
                m.CampanhaId,
                m.UsuarioId,
                NomeUsuario = m.Usuario.Nome,
                m.Conteudo,
                m.DataEnvio,
                m.Fixada
            })
            .ToListAsync();

        return Ok(new { data = mensagens, total = mensagens.Count });
    }

    // POST: api/campanhas/5/chat - Enviar mensagem no chat
    [HttpPost("{id}/chat")]
    public async Task<IActionResult> EnviarMensagemChat(int id, [FromBody] EnviarMensagemRequest request)
    {
        // Verificar se campanha existe
        var campanha = await _context.Campanhas.FindAsync(id);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        // Verificar se o usuário é participante ou moderador/admin
        var usuario = await _context.Usuarios.FindAsync(request.UsuarioId);
        if (usuario == null)
            return BadRequest(new { message = "Usuário não encontrado" });

        var isModerador = usuario.Tipo == "moderador" || usuario.Tipo == "admin";
        
        if (!isModerador)
        {
            var isParticipante = await _context.ParticipantesCampanha
                .AnyAsync(p => p.CampanhaId == id && p.UsuarioId == request.UsuarioId && p.Status == "ativo");

            if (!isParticipante)
                return Unauthorized(new { message = "Você precisa ser voluntário desta campanha para enviar mensagens" });
        }

        var mensagem = new MensagemChat
        {
            CampanhaId = id,
            UsuarioId = request.UsuarioId,
            Conteudo = request.Conteudo,
            DataEnvio = DateTime.UtcNow,
            Fixada = false,
            Deletada = false
        };

        _context.MensagensChat.Add(mensagem);
        await _context.SaveChangesAsync();

        return Ok(new {
            mensagem.Id,
            mensagem.CampanhaId,
            mensagem.UsuarioId,
            NomeUsuario = usuario.Nome,
            mensagem.Conteudo,
            mensagem.DataEnvio,
            mensagem.Fixada
        });
    }
}

// DTO para adicionar participante
public class AdicionarParticipanteRequest
{
    public int UsuarioId { get; set; }
    public string? Tipo { get; set; }
}

// DTO para enviar mensagem
public class EnviarMensagemRequest
{
    public int UsuarioId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
}

// DTO para enviar advertência
public class AdvertenciaParticipanteRequest
{
    public int ModeradorId { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}

// DTO para remover participante da campanha
public class RemoverParticipanteCampanhaRequest
{
    public int ModeradorId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

// DTO para delegar campanha
public class DelegarCampanhaRequest
{
    public int InstituicaoId { get; set; }
}
