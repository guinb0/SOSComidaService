using Microsoft.AspNetCore.Mvc;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Models;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/campanhas")]
public class ModeracaoCampanhasController : ControllerBase
{
    private readonly string _participantesPath;
    private readonly string _mensagensPath;
    private readonly string _avisosPath;
    private readonly string _usuariosPath;

    public ModeracaoCampanhasController(IWebHostEnvironment env)
    {
        _participantesPath = Path.Combine(env.ContentRootPath, "Data", "participantes_campanha.txt");
        _mensagensPath = Path.Combine(env.ContentRootPath, "Data", "mensagens_chat.txt");
        _avisosPath = Path.Combine(env.ContentRootPath, "Data", "avisos_campanha.txt");
        _usuariosPath = Path.Combine(env.ContentRootPath, "Data", "usuarios.txt");
    }

    #region Helpers

    private List<Usuario> LerUsuarios()
    {
        if (!System.IO.File.Exists(_usuariosPath))
            return new List<Usuario>();

        var linhas = System.IO.File.ReadAllLines(_usuariosPath);
        var usuarios = new List<Usuario>();

        foreach (var linha in linhas)
        {
            var partes = linha.Split('|');
            if (partes.Length >= 7)
            {
                usuarios.Add(new Usuario
                {
                    Id = int.Parse(partes[0]),
                    Nome = partes[1],
                    Email = partes[2],
                    Telefone = partes[3],
                    Endereco = partes[4],
                    Cidade = partes[5],
                    Tipo = partes[6]
                });
            }
        }

        return usuarios;
    }

    private List<ParticipanteCampanha> LerParticipantes()
    {
        if (!System.IO.File.Exists(_participantesPath))
            return new List<ParticipanteCampanha>();

        var linhas = System.IO.File.ReadAllLines(_participantesPath);
        var participantes = new List<ParticipanteCampanha>();

        foreach (var linha in linhas)
        {
            var partes = linha.Split('|');
            if (partes.Length >= 7)
            {
                participantes.Add(new ParticipanteCampanha
                {
                    Id = int.Parse(partes[0]),
                    CampanhaId = int.Parse(partes[1]),
                    UsuarioId = int.Parse(partes[2]),
                    Tipo = partes[3],
                    Status = partes[4],
                    DataEntrada = DateTime.Parse(partes[5]),
                    MotivoSaida = string.IsNullOrEmpty(partes[6]) ? null : partes[6]
                });
            }
        }

        return participantes;
    }

    private void SalvarParticipantes(List<ParticipanteCampanha> participantes)
    {
        var linhas = participantes.Select(p =>
            $"{p.Id}|{p.CampanhaId}|{p.UsuarioId}|{p.Tipo}|{p.Status}|{p.DataEntrada}|{p.MotivoSaida ?? ""}"
        ).ToList();
        System.IO.File.WriteAllLines(_participantesPath, linhas);
    }

    private List<MensagemChat> LerMensagens()
    {
        if (!System.IO.File.Exists(_mensagensPath))
            return new List<MensagemChat>();

        var linhas = System.IO.File.ReadAllLines(_mensagensPath);
        var mensagens = new List<MensagemChat>();

        foreach (var linha in linhas)
        {
            var partes = linha.Split('|');
            if (partes.Length >= 6)
            {
                mensagens.Add(new MensagemChat
                {
                    Id = int.Parse(partes[0]),
                    CampanhaId = int.Parse(partes[1]),
                    UsuarioId = int.Parse(partes[2]),
                    Conteudo = partes[3],
                    DataEnvio = DateTime.Parse(partes[4]),
                    Fixada = bool.Parse(partes[5]),
                    Deletada = partes.Length > 6 && bool.Parse(partes[6])
                });
            }
        }

        return mensagens;
    }

    private void SalvarMensagens(List<MensagemChat> mensagens)
    {
        var linhas = mensagens.Select(m =>
            $"{m.Id}|{m.CampanhaId}|{m.UsuarioId}|{m.Conteudo}|{m.DataEnvio}|{m.Fixada}|{m.Deletada}"
        ).ToList();
        System.IO.File.WriteAllLines(_mensagensPath, linhas);
    }

    private List<AvisoCampanha> LerAvisos()
    {
        if (!System.IO.File.Exists(_avisosPath))
            return new List<AvisoCampanha>();

        var linhas = System.IO.File.ReadAllLines(_avisosPath);
        var avisos = new List<AvisoCampanha>();

        foreach (var linha in linhas)
        {
            var partes = linha.Split('|');
            if (partes.Length >= 9)
            {
                avisos.Add(new AvisoCampanha
                {
                    Id = int.Parse(partes[0]),
                    CampanhaId = int.Parse(partes[1]),
                    ModeradorId = int.Parse(partes[2]),
                    DestinatarioId = string.IsNullOrEmpty(partes[3]) ? null : int.Parse(partes[3]),
                    Titulo = partes[4],
                    Mensagem = partes[5],
                    Tipo = partes[6],
                    DataEnvio = DateTime.Parse(partes[7]),
                    Lido = bool.Parse(partes[8])
                });
            }
        }

        return avisos;
    }

    private void SalvarAvisos(List<AvisoCampanha> avisos)
    {
        var linhas = avisos.Select(a =>
            $"{a.Id}|{a.CampanhaId}|{a.ModeradorId}|{a.DestinatarioId?.ToString() ?? ""}|{a.Titulo}|{a.Mensagem}|{a.Tipo}|{a.DataEnvio}|{a.Lido}"
        ).ToList();
        System.IO.File.WriteAllLines(_avisosPath, linhas);
    }

    #endregion

    #region Participantes

    // GET: api/campanhas/{campanhaId}/participantes
    [HttpGet("{campanhaId}/participantes")]
    public ActionResult<IEnumerable<ParticipanteDto>> GetParticipantes(int campanhaId)
    {
        var participantes = LerParticipantes()
            .Where(p => p.CampanhaId == campanhaId)
            .ToList();

        var usuarios = LerUsuarios();

        var participantesDto = participantes.Select(p =>
        {
            var usuario = usuarios.FirstOrDefault(u => u.Id == p.UsuarioId);
            return new ParticipanteDto
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                Nome = usuario?.Nome ?? "Usuário não encontrado",
                Email = usuario?.Email ?? "",
                Telefone = usuario?.Telefone,
                Tipo = p.Tipo,
                Status = p.Status,
                DataEntrada = p.DataEntrada,
                DataSaida = p.DataSaida,
                MotivoSaida = p.MotivoSaida
            };
        }).ToList();

        return Ok(new { data = participantesDto, total = participantesDto.Count });
    }

    // POST: api/campanhas/{campanhaId}/participantes
    [HttpPost("{campanhaId}/participantes")]
    public ActionResult<ParticipanteDto> AdicionarParticipante(int campanhaId, [FromBody] ParticipanteCampanha participante)
    {
        var participantes = LerParticipantes();
        
        // Verificar se já existe
        var existente = participantes.FirstOrDefault(p => 
            p.CampanhaId == campanhaId && p.UsuarioId == participante.UsuarioId && p.Status == "ativo");
        
        if (existente != null)
            return BadRequest(new { message = "Usuário já está participando desta campanha" });

        var novoId = participantes.Any() ? participantes.Max(p => p.Id) + 1 : 1;
        
        var novoParticipante = new ParticipanteCampanha
        {
            Id = novoId,
            CampanhaId = campanhaId,
            UsuarioId = participante.UsuarioId,
            Tipo = participante.Tipo,
            Status = "ativo",
            DataEntrada = DateTime.Now
        };

        participantes.Add(novoParticipante);
        SalvarParticipantes(participantes);

        var usuarios = LerUsuarios();
        var usuario = usuarios.FirstOrDefault(u => u.Id == participante.UsuarioId);

        return Ok(new ParticipanteDto
        {
            Id = novoParticipante.Id,
            UsuarioId = novoParticipante.UsuarioId,
            Nome = usuario?.Nome ?? "Usuário não encontrado",
            Email = usuario?.Email ?? "",
            Telefone = usuario?.Telefone,
            Tipo = novoParticipante.Tipo,
            Status = novoParticipante.Status,
            DataEntrada = novoParticipante.DataEntrada
        });
    }

    // DELETE: api/campanhas/{campanhaId}/participantes/{usuarioId}
    [HttpDelete("{campanhaId}/participantes/{usuarioId}")]
    public ActionResult RemoverParticipante(int campanhaId, int usuarioId, [FromBody] RemoverParticipanteRequest request)
    {
        var participantes = LerParticipantes();
        var participante = participantes.FirstOrDefault(p => 
            p.CampanhaId == campanhaId && p.UsuarioId == usuarioId && p.Status == "ativo");

        if (participante == null)
            return NotFound(new { message = "Participante não encontrado" });

        participante.Status = "removido";
        participante.DataSaida = DateTime.Now;
        participante.MotivoSaida = request.Motivo;
        participante.RemovidoPorId = request.ModeradorId;

        SalvarParticipantes(participantes);

        return Ok(new { message = "Participante removido com sucesso" });
    }

    #endregion

    #region Avisos

    // GET: api/campanhas/{campanhaId}/avisos
    [HttpGet("{campanhaId}/avisos")]
    public ActionResult<IEnumerable<AvisoCampanhaDto>> GetAvisos(int campanhaId)
    {
        var avisos = LerAvisos()
            .Where(a => a.CampanhaId == campanhaId)
            .OrderByDescending(a => a.DataEnvio)
            .ToList();

        var usuarios = LerUsuarios();

        var avisosDto = avisos.Select(a =>
        {
            var moderador = usuarios.FirstOrDefault(u => u.Id == a.ModeradorId);
            var destinatario = a.DestinatarioId.HasValue 
                ? usuarios.FirstOrDefault(u => u.Id == a.DestinatarioId.Value) 
                : null;

            return new AvisoCampanhaDto
            {
                Id = a.Id,
                CampanhaId = a.CampanhaId,
                ModeradorId = a.ModeradorId,
                NomeModerador = moderador?.Nome ?? "Moderador",
                DestinatarioId = a.DestinatarioId,
                NomeDestinatario = destinatario?.Nome,
                Titulo = a.Titulo,
                Mensagem = a.Mensagem,
                Tipo = a.Tipo,
                DataEnvio = a.DataEnvio,
                Lido = a.Lido,
                DataLeitura = a.DataLeitura
            };
        }).ToList();

        return Ok(new { data = avisosDto, total = avisosDto.Count });
    }

    // GET: api/campanhas/avisos/usuario/{usuarioId}
    [HttpGet("avisos/usuario/{usuarioId}")]
    public ActionResult<IEnumerable<AvisoCampanhaDto>> GetAvisosUsuario(int usuarioId)
    {
        var avisos = LerAvisos()
            .Where(a => a.DestinatarioId == usuarioId || a.DestinatarioId == null)
            .OrderByDescending(a => a.DataEnvio)
            .ToList();

        var usuarios = LerUsuarios();

        var avisosDto = avisos.Select(a =>
        {
            var moderador = usuarios.FirstOrDefault(u => u.Id == a.ModeradorId);

            return new AvisoCampanhaDto
            {
                Id = a.Id,
                CampanhaId = a.CampanhaId,
                ModeradorId = a.ModeradorId,
                NomeModerador = moderador?.Nome ?? "Moderador",
                DestinatarioId = a.DestinatarioId,
                Titulo = a.Titulo,
                Mensagem = a.Mensagem,
                Tipo = a.Tipo,
                DataEnvio = a.DataEnvio,
                Lido = a.Lido,
                DataLeitura = a.DataLeitura
            };
        }).ToList();

        return Ok(new { data = avisosDto, total = avisosDto.Count });
    }

    // POST: api/campanhas/{campanhaId}/avisos
    [HttpPost("{campanhaId}/avisos")]
    public ActionResult<AvisoCampanhaDto> EnviarAviso(int campanhaId, [FromBody] EnviarAvisoRequest request)
    {
        var avisos = LerAvisos();
        var novoId = avisos.Any() ? avisos.Max(a => a.Id) + 1 : 1;

        var novoAviso = new AvisoCampanha
        {
            Id = novoId,
            CampanhaId = campanhaId,
            ModeradorId = request.ModeradorId,
            DestinatarioId = request.DestinatarioId,
            Titulo = request.Titulo,
            Mensagem = request.Mensagem,
            Tipo = request.Tipo,
            DataEnvio = DateTime.Now,
            Lido = false
        };

        avisos.Add(novoAviso);
        SalvarAvisos(avisos);

        var usuarios = LerUsuarios();
        var moderador = usuarios.FirstOrDefault(u => u.Id == request.ModeradorId);
        var destinatario = request.DestinatarioId.HasValue 
            ? usuarios.FirstOrDefault(u => u.Id == request.DestinatarioId.Value) 
            : null;

        return Ok(new AvisoCampanhaDto
        {
            Id = novoAviso.Id,
            CampanhaId = novoAviso.CampanhaId,
            ModeradorId = novoAviso.ModeradorId,
            NomeModerador = moderador?.Nome ?? "Moderador",
            DestinatarioId = novoAviso.DestinatarioId,
            NomeDestinatario = destinatario?.Nome,
            Titulo = novoAviso.Titulo,
            Mensagem = novoAviso.Mensagem,
            Tipo = novoAviso.Tipo,
            DataEnvio = novoAviso.DataEnvio,
            Lido = novoAviso.Lido
        });
    }

    // PUT: api/campanhas/avisos/{avisoId}/lido
    [HttpPut("avisos/{avisoId}/lido")]
    public ActionResult MarcarAvisoComoLido(int avisoId)
    {
        var avisos = LerAvisos();
        var aviso = avisos.FirstOrDefault(a => a.Id == avisoId);

        if (aviso == null)
            return NotFound(new { message = "Aviso não encontrado" });

        aviso.Lido = true;
        aviso.DataLeitura = DateTime.Now;

        SalvarAvisos(avisos);

        return Ok(new { message = "Aviso marcado como lido" });
    }

    #endregion

    #region Chat

    // GET: api/campanhas/{campanhaId}/chat
    [HttpGet("{campanhaId}/chat")]
    public ActionResult<IEnumerable<MensagemChatDto>> GetMensagensChat(int campanhaId, [FromQuery] int? ultimas = 50)
    {
        var mensagens = LerMensagens()
            .Where(m => m.CampanhaId == campanhaId && !m.Deletada)
            .OrderByDescending(m => m.DataEnvio)
            .Take(ultimas ?? 50)
            .Reverse()
            .ToList();

        var usuarios = LerUsuarios();

        var mensagensDto = mensagens.Select(m =>
        {
            var usuario = usuarios.FirstOrDefault(u => u.Id == m.UsuarioId);
            var fixadaPor = m.FixadaPorId.HasValue 
                ? usuarios.FirstOrDefault(u => u.Id == m.FixadaPorId.Value) 
                : null;

            return new MensagemChatDto
            {
                Id = m.Id,
                CampanhaId = m.CampanhaId,
                UsuarioId = m.UsuarioId,
                NomeUsuario = usuario?.Nome ?? "Usuário",
                Conteudo = m.Conteudo,
                DataEnvio = m.DataEnvio,
                DataEdicao = m.DataEdicao,
                Fixada = m.Fixada,
                FixadaPorNome = fixadaPor?.Nome
            };
        }).ToList();

        return Ok(new { data = mensagensDto, total = mensagensDto.Count });
    }

    // POST: api/campanhas/{campanhaId}/chat
    [HttpPost("{campanhaId}/chat")]
    public ActionResult<MensagemChatDto> EnviarMensagem(int campanhaId, [FromBody] EnviarMensagemRequest request)
    {
        // Verificar se o usuário é participante da campanha
        var participantes = LerParticipantes();
        var isParticipante = participantes.Any(p => 
            p.CampanhaId == campanhaId && p.UsuarioId == request.UsuarioId && p.Status == "ativo");

        // Verificar se é moderador
        var usuarios = LerUsuarios();
        var usuario = usuarios.FirstOrDefault(u => u.Id == request.UsuarioId);
        var isModerador = usuario?.Tipo == "moderador" || usuario?.Email == "guinb@soscomida.com";

        if (!isParticipante && !isModerador)
            return Unauthorized(new { message = "Você não tem permissão para enviar mensagens nesta campanha" });

        var mensagens = LerMensagens();
        var novoId = mensagens.Any() ? mensagens.Max(m => m.Id) + 1 : 1;

        var novaMensagem = new MensagemChat
        {
            Id = novoId,
            CampanhaId = campanhaId,
            UsuarioId = request.UsuarioId,
            Conteudo = request.Conteudo,
            DataEnvio = DateTime.Now,
            Fixada = false,
            Deletada = false
        };

        mensagens.Add(novaMensagem);
        SalvarMensagens(mensagens);

        return Ok(new MensagemChatDto
        {
            Id = novaMensagem.Id,
            CampanhaId = novaMensagem.CampanhaId,
            UsuarioId = novaMensagem.UsuarioId,
            NomeUsuario = usuario?.Nome ?? "Usuário",
            Conteudo = novaMensagem.Conteudo,
            DataEnvio = novaMensagem.DataEnvio,
            Fixada = novaMensagem.Fixada
        });
    }

    // DELETE: api/campanhas/{campanhaId}/chat/{mensagemId}
    [HttpDelete("{campanhaId}/chat/{mensagemId}")]
    public ActionResult DeletarMensagem(int campanhaId, int mensagemId, [FromQuery] int moderadorId)
    {
        var mensagens = LerMensagens();
        var mensagem = mensagens.FirstOrDefault(m => m.Id == mensagemId && m.CampanhaId == campanhaId);

        if (mensagem == null)
            return NotFound(new { message = "Mensagem não encontrada" });

        mensagem.Deletada = true;

        SalvarMensagens(mensagens);

        return Ok(new { message = "Mensagem deletada com sucesso" });
    }

    // PUT: api/campanhas/{campanhaId}/chat/{mensagemId}/fixar
    [HttpPut("{campanhaId}/chat/{mensagemId}/fixar")]
    public ActionResult FixarMensagem(int campanhaId, int mensagemId, [FromQuery] int moderadorId)
    {
        var mensagens = LerMensagens();
        var mensagem = mensagens.FirstOrDefault(m => m.Id == mensagemId && m.CampanhaId == campanhaId);

        if (mensagem == null)
            return NotFound(new { message = "Mensagem não encontrada" });

        mensagem.Fixada = !mensagem.Fixada;
        mensagem.FixadaPorId = mensagem.Fixada ? moderadorId : null;

        SalvarMensagens(mensagens);

        return Ok(new { message = mensagem.Fixada ? "Mensagem fixada" : "Mensagem desafixada" });
    }

    #endregion
}
