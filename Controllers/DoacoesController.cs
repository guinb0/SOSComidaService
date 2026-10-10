using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.Seguranca;

namespace SOSComida.Controllers;

/// <summary>
/// Doações em dinheiro.
///
/// Ainda não há gateway de pagamento: a doação é registrada como confirmada
/// na hora, como a tela sempre fez. A diferença é que antes nada era gravado —
/// a tela dizia "confirmada" e o histórico de doações era uma lista fixa.
/// Quando entrar um gateway, a doação nasce <c>pendente</c> e o webhook dele
/// confirma (e só então soma no arrecadado).
/// </summary>
[ApiController]
public class DoacoesController : ControllerBase
{
    private static readonly string[] Metodos = { "pix", "credito", "debito" };

    private readonly ApplicationDbContext _context;

    public DoacoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/campanhas/5/doacoes
    [HttpPost("api/campanhas/{campanhaId:int}/doacoes")]
    public async Task<IActionResult> Doar(int campanhaId, [FromBody] NovaDoacaoRequest request)
    {
        var campanha = await _context.Campanhas.FindAsync(campanhaId);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        if (!campanha.Ativa)
            return BadRequest(new { message = "Esta campanha não está recebendo doações" });

        var metodo = request.MetodoPagamento?.Trim().ToLowerInvariant() ?? "pix";
        if (!Metodos.Contains(metodo))
            return BadRequest(new { message = "Forma de pagamento inválida" });

        var doacao = await RegistrarAsync(_context, campanha, User.Id(), request.Valor, metodo);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Doação registrada. Obrigado por ajudar!",
            doacao = new { doacao.Id, doacao.Valor, doacao.Status, doacao.DataDoacao },
            valorArrecadado = campanha.ValorArrecadado
        });
    }

    // GET: api/doacoes/minhas
    [HttpGet("api/doacoes/minhas")]
    public async Task<IActionResult> Minhas()
    {
        var usuarioId = User.Id();
        var doacoes = await _context.Doacoes
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.DataDoacao)
            .Select(d => new
            {
                d.Id,
                d.CampanhaId,
                CampanhaTitulo = d.Campanha.Titulo,
                CampanhaImagem = d.Campanha.ImagemUrl,
                d.Valor,
                Data = d.DataDoacao,
                d.Status,
                d.MetodoPagamento
            })
            .ToListAsync();

        return Ok(new { data = doacoes, total = doacoes.Count });
    }

    /// <summary>
    /// Grava a doação, soma no arrecadado e inscreve o doador na campanha (é
    /// isso que libera o chat para ele). Quem chama faz o SaveChanges.
    /// </summary>
    internal static async Task<Doacao> RegistrarAsync(
        ApplicationDbContext context, Campanha campanha, int usuarioId, decimal valor, string? metodo)
    {
        var agora = DateTime.UtcNow;
        var doacao = new Doacao
        {
            CampanhaId = campanha.Id,
            UsuarioId = usuarioId,
            Valor = valor,
            Tipo = "dinheiro",
            MetodoPagamento = metodo,
            Status = StatusDoacoes.Confirmada,
            DataDoacao = agora,
            DataConfirmacao = agora
        };
        context.Doacoes.Add(doacao);

        campanha.ValorArrecadado += valor;
        campanha.DataAtualizacao = agora;

        var jaParticipa = await context.ParticipantesCampanha.AnyAsync(p =>
            p.CampanhaId == campanha.Id && p.UsuarioId == usuarioId && p.Status == StatusParticipantes.Ativo);
        if (!jaParticipa)
        {
            context.ParticipantesCampanha.Add(new ParticipanteCampanha
            {
                CampanhaId = campanha.Id,
                UsuarioId = usuarioId,
                Tipo = "doador",
                Status = StatusParticipantes.Ativo,
                DataEntrada = agora
            });
        }

        return doacao;
    }
}

public class NovaDoacaoRequest
{
    [Range(1, 1_000_000, ErrorMessage = "O valor da doação deve ficar entre R$ 1 e R$ 1.000.000")]
    public decimal Valor { get; set; }

    public string? MetodoPagamento { get; set; }
}
