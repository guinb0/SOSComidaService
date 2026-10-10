using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;

namespace SOSComida.Controllers;

/// <summary>
/// Visão consolidada da prestação de contas: todas as campanhas que já têm
/// nota fiscal enviada, com o que foi comprovado em cada uma.
///
/// Leitura pública de propósito — é a tela de transparência, e exigir login
/// para ver onde o dinheiro foi parar contraria o objetivo dela.
/// </summary>
[ApiController]
[Route("api/prestacao-contas")]
[AllowAnonymous]
public class PrestacaoContasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PrestacaoContasController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <param name="usuarioId">
    /// Opcional: restringe às campanhas criadas por esse usuário. Filtrar
    /// pelas campanhas em que ele *doou* ainda não é possível — não existe
    /// endpoint de doações, a tela de histórico é mock.
    /// </param>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? usuarioId)
    {
        var query = _context.NotasFiscais
            .Include(n => n.Campanha)
            .Include(n => n.EnviadoPor)
            .AsQueryable();

        if (usuarioId.HasValue)
            query = query.Where(n => n.Campanha.UsuarioId == usuarioId
                                  || n.Campanha.InstituicaoId == usuarioId);

        var notas = await query.OrderByDescending(n => n.DataEnvio).ToListAsync();

        var porCampanha = notas
            .GroupBy(n => n.Campanha)
            .Select(g => new
            {
                campanhaId = g.Key.Id,
                campanhaTitulo = g.Key.Titulo,
                valorArrecadado = g.Key.ValorArrecadado,
                totalComprovado = g.Where(n => n.Revisado && n.ValorTotal.HasValue)
                                   .Sum(n => n.ValorTotal!.Value),
                aguardandoRevisao = g.Count(n => !n.Revisado),
                ultimoEnvio = g.Max(n => n.DataEnvio),
                notas = g.OrderByDescending(n => n.DataEnvio).Select(n => new
                {
                    n.Id,
                    n.ImagemUrl,
                    n.Descricao,
                    n.ValorTotal,
                    n.CnpjEmitente,
                    n.NomeEmitente,
                    n.NumeroNota,
                    n.DataEmissao,
                    n.ChaveAcesso,
                    n.ConfiancaOcr,
                    n.Revisado,
                    n.DataEnvio,
                    enviadoPor = n.EnviadoPor == null
                        ? null
                        : new { n.EnviadoPor.Id, n.EnviadoPor.Nome, n.EnviadoPor.Tipo }
                })
            })
            .OrderByDescending(c => c.ultimoEnvio)
            .ToList();

        return Ok(new
        {
            totalCampanhas = porCampanha.Count,
            campanhas = porCampanha
        });
    }
}
