using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.Seguranca;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/participacoes")]
public class ParticipacoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ParticipacoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Campanhas em que o usuário logado se inscreveu como voluntário — base
    /// do histórico de voluntariado, que antes era uma lista fixa de exemplo.
    /// </summary>
    // GET: api/participacoes/minhas
    [HttpGet("minhas")]
    public async Task<IActionResult> Minhas()
    {
        var usuarioId = User.Id();
        var participacoes = await _context.ParticipantesCampanha
            .Where(p => p.UsuarioId == usuarioId && p.Tipo == "voluntario")
            .OrderByDescending(p => p.DataEntrada)
            .Select(p => new
            {
                p.Id,
                p.CampanhaId,
                CampanhaTitulo = p.Campanha.Titulo,
                CampanhaImagem = p.Campanha.ImagemUrl,
                CampanhaStatus = p.Campanha.Status,
                Local = p.Campanha.Localizacao,
                DataInscricao = p.DataEntrada,
                DataInicio = p.Campanha.DataInicio,
                DataFim = p.Campanha.DataFim,
                p.Status,
                p.DataSaida,
                p.MotivoSaida
            })
            .ToListAsync();

        var resultado = participacoes.Select(p => new
        {
            p.Id,
            p.CampanhaId,
            p.CampanhaTitulo,
            p.CampanhaImagem,
            p.Local,
            p.DataInscricao,
            p.DataInicio,
            p.DataFim,
            p.MotivoSaida,
            // removido → cancelado; ativo numa campanha encerrada → concluido.
            Status = p.Status == StatusParticipantes.Removido
                ? "cancelado"
                : p.CampanhaStatus == StatusCampanhas.Finalizada || p.DataFim < DateTime.UtcNow
                    ? "concluido"
                    : "ativo"
        }).ToList();

        return Ok(new { data = resultado, total = resultado.Count });
    }
}
