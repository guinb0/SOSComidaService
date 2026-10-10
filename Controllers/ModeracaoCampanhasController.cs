using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Models;
using SOSComida.Seguranca;
using SOSComida.Services;

namespace SOSComida.Controllers;

/// <summary>
/// Avisos dos moderadores para os participantes de uma campanha.
///
/// Antes este controller lia e gravava Data/*.txt, com uma lista de usuários
/// própria que não era a do banco — e regravar o arquivo inteiro a cada
/// requisição perdia dados quando duas chegavam juntas. Participantes e chat
/// já viviam no banco (CampanhasController); agora os avisos também.
/// </summary>
[ApiController]
[Route("api/moderacao/campanhas")]
public class ModeracaoCampanhasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ModeracaoCampanhasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/moderacao/campanhas/5/avisos
    [HttpGet("{campanhaId}/avisos")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> GetAvisos(int campanhaId)
    {
        var avisos = await Projetar(_context.AvisosCampanha.Where(a => a.CampanhaId == campanhaId))
            .ToListAsync();

        return Ok(new { data = avisos, total = avisos.Count });
    }

    // GET: api/moderacao/campanhas/avisos/meus - Avisos do usuário logado
    [HttpGet("avisos/meus")]
    public async Task<IActionResult> GetMeusAvisos()
    {
        var usuarioId = User.Id();

        // Aviso "para todos" só vale para quem participa daquela campanha.
        var campanhasDoUsuario = _context.ParticipantesCampanha
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => p.CampanhaId);

        var avisos = await Projetar(_context.AvisosCampanha.Where(a =>
                a.DestinatarioId == usuarioId
                || (a.DestinatarioId == null && campanhasDoUsuario.Contains(a.CampanhaId))))
            .ToListAsync();

        return Ok(new { data = avisos, total = avisos.Count });
    }

    // POST: api/moderacao/campanhas/5/avisos
    [HttpPost("{campanhaId}/avisos")]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> EnviarAviso(int campanhaId, [FromBody] EnviarAvisoRequest request)
    {
        if (!await _context.Campanhas.AnyAsync(c => c.Id == campanhaId))
            return NotFound(new { message = "Campanha não encontrada" });

        var aviso = new AvisoCampanha
        {
            CampanhaId = campanhaId,
            ModeradorId = User.Id(),
            DestinatarioId = request.DestinatarioId,
            Titulo = request.Titulo,
            Mensagem = request.Mensagem,
            Tipo = request.Tipo,
            DataEnvio = DateTime.UtcNow,
            Lido = false
        };

        _context.AvisosCampanha.Add(aviso);
        _context.Registrar(User.Id(), AcoesAuditadas.AvisoEnviado, ItensAuditados.Campanha, campanhaId,
            $"Enviou o aviso \"{request.Titulo}\"", request.Mensagem);
        await _context.SaveChangesAsync();

        var dto = await Projetar(_context.AvisosCampanha.Where(a => a.Id == aviso.Id)).FirstAsync();
        return Ok(dto);
    }

    // PUT: api/moderacao/campanhas/avisos/10/lido
    [HttpPut("avisos/{avisoId}/lido")]
    public async Task<IActionResult> MarcarAvisoComoLido(int avisoId)
    {
        var aviso = await _context.AvisosCampanha.FindAsync(avisoId);

        if (aviso == null)
            return NotFound(new { message = "Aviso não encontrado" });

        if (aviso.DestinatarioId != null && aviso.DestinatarioId != User.Id())
            return Forbid();

        aviso.Lido = true;
        aviso.DataLeitura = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Aviso marcado como lido" });
    }

    private static IQueryable<AvisoCampanhaDto> Projetar(IQueryable<AvisoCampanha> query) =>
        query
            .OrderByDescending(a => a.DataEnvio)
            .Select(a => new AvisoCampanhaDto
            {
                Id = a.Id,
                CampanhaId = a.CampanhaId,
                TituloCampanha = a.Campanha.Titulo,
                ModeradorId = a.ModeradorId,
                NomeModerador = a.Moderador.Nome,
                DestinatarioId = a.DestinatarioId,
                NomeDestinatario = a.Destinatario != null ? a.Destinatario.Nome : null,
                Titulo = a.Titulo,
                Mensagem = a.Mensagem,
                Tipo = a.Tipo,
                DataEnvio = a.DataEnvio,
                Lido = a.Lido,
                DataLeitura = a.DataLeitura
            });
}
