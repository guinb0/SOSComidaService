using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/notificacoes")]
public class NotificacoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificacoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/notificacoes/usuario/5 - Buscar notificações do usuário
    [HttpGet("usuario/{usuarioId}")]
    public async Task<IActionResult> GetNotificacoesUsuario(int usuarioId)
    {
        var notificacoes = await _context.Notificacoes
            .Include(n => n.Campanha)
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.DataCriacao)
            .Select(n => new {
                n.Id,
                n.Tipo,
                n.Titulo,
                n.Mensagem,
                n.Lida,
                n.DataCriacao,
                n.CampanhaId,
                CampanhaTitulo = n.Campanha != null ? n.Campanha.Titulo : null,
                n.StatusDelegacao
            })
            .ToListAsync();

        var naoLidas = notificacoes.Count(n => !n.Lida);

        return Ok(new { data = notificacoes, total = notificacoes.Count, naoLidas });
    }

    // PUT: api/notificacoes/5/lida - Marcar como lida
    [HttpPut("{id}/lida")]
    public async Task<IActionResult> MarcarComoLida(int id)
    {
        var notificacao = await _context.Notificacoes.FindAsync(id);
        if (notificacao == null)
            return NotFound(new { message = "Notificação não encontrada" });

        notificacao.Lida = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Notificação marcada como lida" });
    }

    // PUT: api/notificacoes/usuario/5/marcar-todas-lidas
    [HttpPut("usuario/{usuarioId}/marcar-todas-lidas")]
    public async Task<IActionResult> MarcarTodasComoLidas(int usuarioId)
    {
        var notificacoes = await _context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId && !n.Lida)
            .ToListAsync();

        foreach (var n in notificacoes)
        {
            n.Lida = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"{notificacoes.Count} notificações marcadas como lidas" });
    }

    // POST: api/notificacoes/5/responder-delegacao - Aceitar ou recusar delegação
    [HttpPost("{id}/responder-delegacao")]
    public async Task<IActionResult> ResponderDelegacao(int id, [FromBody] ResponderDelegacaoRequest request)
    {
        var notificacao = await _context.Notificacoes
            .Include(n => n.Campanha)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notificacao == null)
            return NotFound(new { message = "Notificação não encontrada" });

        if (notificacao.Tipo != "delegacao_campanha")
            return BadRequest(new { message = "Esta notificação não é uma delegação" });

        if (notificacao.StatusDelegacao != "pendente")
            return BadRequest(new { message = "Esta delegação já foi respondida" });

        notificacao.StatusDelegacao = request.Aceitar ? "aceita" : "recusada";
        notificacao.Lida = true;

        if (notificacao.CampanhaId != null)
        {
            var campanha = await _context.Campanhas.FindAsync(notificacao.CampanhaId);
            if (campanha != null)
            {
                if (request.Aceitar)
                {
                    campanha.StatusDelegacao = "aceita";
                    campanha.Status = "ativa";
                    campanha.Ativa = true;
                }
                else
                {
                    campanha.StatusDelegacao = "recusada";
                    campanha.InstituicaoId = null;
                    // Volta para pendente se a instituição recusou
                    campanha.Status = "pendente";
                }
                campanha.DataAtualizacao = DateTime.UtcNow;
            }
        }

        // Notificar o criador da campanha sobre a resposta
        if (notificacao.CampanhaId != null)
        {
            var campanha = await _context.Campanhas.FindAsync(notificacao.CampanhaId);
            if (campanha?.UsuarioId != null)
            {
                var instituicao = await _context.Usuarios.FindAsync(notificacao.UsuarioId);
                var novaNotificacao = new Notificacao
                {
                    UsuarioId = campanha.UsuarioId.Value,
                    Tipo = "resposta_delegacao",
                    Titulo = request.Aceitar ? "Campanha aceita pela instituição!" : "Instituição recusou sua campanha",
                    Mensagem = request.Aceitar 
                        ? $"A instituição {instituicao?.Nome} aceitou gerenciar sua campanha '{campanha.Titulo}'."
                        : $"A instituição {instituicao?.Nome} recusou gerenciar sua campanha '{campanha.Titulo}'. A campanha voltou para análise.",
                    CampanhaId = campanha.Id,
                    StatusDelegacao = request.Aceitar ? "aceita" : "recusada"
                };
                _context.Notificacoes.Add(novaNotificacao);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { 
            message = request.Aceitar ? "Delegação aceita com sucesso!" : "Delegação recusada",
            status = notificacao.StatusDelegacao
        });
    }
}

public class ResponderDelegacaoRequest
{
    public bool Aceitar { get; set; }
}
