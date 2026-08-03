using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.Services;

namespace SOSComida.Controllers;

/// <summary>
/// Prestação de contas de uma campanha: as notas fiscais do que foi comprado
/// com o dinheiro arrecadado.
///
/// Quem pode enviar é só quem responde pela campanha — o criador, a
/// instituição para quem ela foi delegada, ou um moderador/admin. A leitura é
/// pública de propósito: o doador precisa poder conferir onde o dinheiro foi
/// parar sem depender de permissão.
/// </summary>
[ApiController]
[Route("api/campanhas/{campanhaId:int}/notas-fiscais")]
public class NotasFiscaisController : ControllerBase
{
    private static readonly string[] TiposPermitidos =
        { "image/jpeg", "image/jpg", "image/png", "image/webp" };

    private const long TamanhoMaximo = 10 * 1024 * 1024; // 10 MB

    private readonly ApplicationDbContext _context;
    private readonly IOcrService _ocr;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(
        ApplicationDbContext context,
        IOcrService ocr,
        ILogger<NotasFiscaisController> logger)
    {
        _context = context;
        _ocr = ocr;
        _logger = logger;
    }

    /// <summary>Lista as notas de uma campanha e o resumo da prestação de contas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(int campanhaId)
    {
        var campanha = await _context.Campanhas.FindAsync(campanhaId);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        var notas = await _context.NotasFiscais
            .Where(n => n.CampanhaId == campanhaId)
            .Include(n => n.EnviadoPor)
            .OrderByDescending(n => n.DataEnvio)
            .ToListAsync();

        // Só nota revisada entra na soma. Valor cru de OCR não pode virar
        // "comprovado" sem alguém ter olhado.
        var totalComprovado = notas
            .Where(n => n.Revisado && n.ValorTotal.HasValue)
            .Sum(n => n.ValorTotal!.Value);

        var aguardandoRevisao = notas.Count(n => !n.Revisado);

        return Ok(new
        {
            campanhaId,
            valorArrecadado = campanha.ValorArrecadado,
            totalComprovado,
            saldoNaoComprovado = campanha.ValorArrecadado - totalComprovado,
            aguardandoRevisao,
            notas = notas.Select(Projetar)
        });
    }

    /// <summary>
    /// Envia a foto da nota, roda o OCR e devolve os campos que deu para
    /// deduzir. Os campos voltam para o responsável conferir — nada aqui é
    /// dado como certo.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(TamanhoMaximo)]
    public async Task<IActionResult> Enviar(int campanhaId, [FromForm] EnviarNotaRequest form)
    {
        var imagem = form.Imagem;
        var usuarioId = form.UsuarioId;
        var descricao = form.Descricao;

        var campanha = await _context.Campanhas.FindAsync(campanhaId);
        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return NotFound(new { message = "Usuário não encontrado" });

        if (!PodeResponderPor(campanha, usuario))
            return StatusCode(403, new
            {
                message = "Só o responsável pela campanha pode enviar comprovantes de gasto"
            });

        if (imagem == null || imagem.Length == 0)
            return BadRequest(new { message = "Nenhuma imagem enviada" });

        if (!TiposPermitidos.Contains(imagem.ContentType.ToLower()))
            return BadRequest(new { message = "Tipo de arquivo não permitido. Use JPG, PNG ou WebP." });

        if (imagem.Length > TamanhoMaximo)
            return BadRequest(new { message = "A imagem deve ter no máximo 10MB" });

        // Grava primeiro: a imagem é a prova de verdade, e ela precisa
        // sobreviver mesmo que o OCR falhe.
        var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "notas");
        Directory.CreateDirectory(pasta);

        var extensao = Path.GetExtension(imagem.FileName);
        if (string.IsNullOrWhiteSpace(extensao)) extensao = ".jpg";
        var nomeArquivo = $"{campanhaId}_{Guid.NewGuid()}{extensao}";
        var caminho = Path.Combine(pasta, nomeArquivo);

        await using (var stream = new FileStream(caminho, FileMode.Create))
        {
            await imagem.CopyToAsync(stream);
        }

        var nota = new NotaFiscal
        {
            CampanhaId = campanhaId,
            EnviadoPorId = usuarioId,
            ImagemUrl = $"/uploads/notas/{nomeArquivo}",
            Descricao = descricao,
            DataEnvio = DateTime.UtcNow
        };

        // OCR não derruba o envio. Se falhar, a nota entra sem campos e o
        // responsável preenche à mão — melhor que perder o comprovante.
        try
        {
            var resultado = await Task.Run(() => _ocr.Ler(caminho));
            var campos = ExtratorNotaFiscal.Extrair(resultado.Texto);

            nota.TextoOcr = resultado.Texto;
            nota.ConfiancaOcr = resultado.Confianca;
            nota.ValorTotal = campos.ValorTotal;
            nota.CnpjEmitente = campos.CnpjEmitente;
            nota.NomeEmitente = campos.NomeEmitente;
            nota.NumeroNota = campos.NumeroNota;
            nota.DataEmissao = campos.DataEmissao;
            nota.ChaveAcesso = campos.ChaveAcesso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR falhou para a nota da campanha {CampanhaId}", campanhaId);
        }

        _context.NotasFiscais.Add(nota);
        await _context.SaveChangesAsync();

        await _context.Entry(nota).Reference(n => n.EnviadoPor).LoadAsync();

        return Ok(new
        {
            message = nota.TextoOcr == null
                ? "Nota enviada, mas não foi possível ler o texto. Preencha os campos manualmente."
                : "Nota enviada. Confira os campos lidos antes de confirmar.",
            nota = Projetar(nota)
        });
    }

    /// <summary>
    /// Corrige os campos lidos pelo OCR e marca a nota como revisada. É este
    /// passo que faz o valor entrar na soma da prestação de contas.
    /// </summary>
    [HttpPut("{notaId:int}")]
    public async Task<IActionResult> Revisar(
        int campanhaId, int notaId, [FromBody] RevisarNotaRequest request)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Campanha)
            .Include(n => n.EnviadoPor)
            .FirstOrDefaultAsync(n => n.Id == notaId && n.CampanhaId == campanhaId);

        if (nota == null)
            return NotFound(new { message = "Nota não encontrada" });

        var usuario = await _context.Usuarios.FindAsync(request.UsuarioId);
        if (usuario == null || !PodeResponderPor(nota.Campanha, usuario))
            return StatusCode(403, new { message = "Sem permissão para revisar esta nota" });

        if (request.ValorTotal is <= 0)
            return BadRequest(new { message = "O valor total deve ser maior que zero" });

        nota.ValorTotal = request.ValorTotal;
        nota.NomeEmitente = request.NomeEmitente;
        nota.CnpjEmitente = request.CnpjEmitente;
        nota.NumeroNota = request.NumeroNota;
        nota.DataEmissao = request.DataEmissao?.ToUniversalTime();
        nota.Descricao = request.Descricao;
        nota.Revisado = true;
        nota.DataRevisao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Nota confirmada", nota = Projetar(nota) });
    }

    [HttpDelete("{notaId:int}")]
    public async Task<IActionResult> Excluir(int campanhaId, int notaId, [FromQuery] int usuarioId)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Campanha)
            .FirstOrDefaultAsync(n => n.Id == notaId && n.CampanhaId == campanhaId);

        if (nota == null)
            return NotFound(new { message = "Nota não encontrada" });

        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null || !PodeResponderPor(nota.Campanha, usuario))
            return StatusCode(403, new { message = "Sem permissão para excluir esta nota" });

        var caminho = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", nota.ImagemUrl.TrimStart('/'));
        if (System.IO.File.Exists(caminho))
            System.IO.File.Delete(caminho);

        _context.NotasFiscais.Remove(nota);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Nota excluída" });
    }

    /// <summary>
    /// Criador da campanha, instituição que aceitou a delegação, moderador ou
    /// admin. Ninguém mais presta contas por uma campanha que não é sua.
    /// </summary>
    private static bool PodeResponderPor(Campanha campanha, Usuario usuario)
    {
        if (usuario.Tipo is "admin" or "moderador") return true;
        if (campanha.UsuarioId == usuario.Id) return true;

        return campanha.InstituicaoId == usuario.Id
            && campanha.StatusDelegacao == "aceita";
    }

    private static object Projetar(NotaFiscal n) => new
    {
        n.Id,
        n.CampanhaId,
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
        n.DataRevisao,
        n.TextoOcr,
        enviadoPor = n.EnviadoPor == null
            ? null
            : new { n.EnviadoPor.Id, n.EnviadoPor.Nome, n.EnviadoPor.Tipo }
    };
}

/// <summary>
/// Campos do multipart do envio de nota.
///
/// Precisam vir agrupados num tipo: com `IFormFile` e `[FromForm]` soltos na
/// assinatura, o Swashbuckle não consegue gerar o schema e derruba o
/// /swagger inteiro — a documentação da API parava de abrir por causa disso.
/// </summary>
public class EnviarNotaRequest
{
    public IFormFile Imagem { get; set; } = null!;
    public int UsuarioId { get; set; }
    public string? Descricao { get; set; }
}

public class RevisarNotaRequest
{
    public int UsuarioId { get; set; }
    public decimal? ValorTotal { get; set; }
    public string? NomeEmitente { get; set; }
    public string? CnpjEmitente { get; set; }
    public string? NumeroNota { get; set; }
    public DateTime? DataEmissao { get; set; }
    public string? Descricao { get; set; }
}
