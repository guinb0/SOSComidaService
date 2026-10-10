using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SOSComida.Services;

/// <summary>
/// Configuração do OCR (seção "Ocr"). Com <see cref="ChaveApi"/> preenchida a
/// API usa o provedor em nuvem; sem chave, o Tesseract local.
/// </summary>
public class OcrSettings
{
    /// <summary>"mistral" ou "tesseract". Vazio: mistral se houver chave, senão tesseract.</summary>
    public string Provedor { get; set; } = string.Empty;

    public string ChaveApi { get; set; } = string.Empty;
    public string Modelo { get; set; } = "mistral-ocr-latest";
    public string UrlBase { get; set; } = "https://api.mistral.ai/v1";
    public int TimeoutSegundos { get; set; } = 60;

    public bool UsaMistral => Provedor.Trim().ToLowerInvariant() switch
    {
        "mistral" => true,
        "tesseract" => false,
        _ => !string.IsNullOrWhiteSpace(ChaveApi)
    };
}

/// <summary>
/// OCR pela API da Mistral (endpoint /ocr). Lê bem melhor que o Tesseract
/// cupom torto ou com sombra, e não depende de biblioteca nativa — funciona
/// no container Linux, onde o Tesseract do NuGet não carrega.
///
/// A resposta vem em Markdown (tabelas com "|", títulos com "#"); o texto é
/// achatado em linhas simples porque o ExtratorNotaFiscal trabalha linha a
/// linha. A API não devolve confiança, então ela vai nula.
/// </summary>
public partial class MistralOcrService : IOcrService
{
    private readonly HttpClient _http;
    private readonly OcrSettings _settings;
    private readonly ILogger<MistralOcrService> _logger;

    public MistralOcrService(HttpClient http, OcrSettings settings, ILogger<MistralOcrService> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task<ResultadoOcr> LerAsync(string caminhoImagem, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ChaveApi))
            throw new InvalidOperationException("Ocr:ChaveApi não configurada.");

        var bytes = await File.ReadAllBytesAsync(caminhoImagem, ct);
        var dataUrl = $"data:{TipoMime(caminhoImagem)};base64,{Convert.ToBase64String(bytes)}";

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"{_settings.UrlBase.TrimEnd('/')}/ocr")
        {
            Content = JsonContent.Create(new
            {
                model = _settings.Modelo,
                document = new { type = "image_url", image_url = dataUrl }
            })
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ChaveApi);

        using var resposta = await _http.SendAsync(requisicao, ct);
        if (!resposta.IsSuccessStatusCode)
        {
            var corpo = await resposta.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"OCR Mistral respondeu {(int)resposta.StatusCode}: {corpo[..Math.Min(corpo.Length, 300)]}");
        }

        var resultado = await resposta.Content.ReadFromJsonAsync<RespostaOcr>(ct);
        var markdown = string.Join("\n", resultado?.Pages.Select(p => p.Markdown) ?? []);
        var texto = ParaTextoSimples(markdown);

        _logger.LogInformation("OCR Mistral concluído: {Chars} caracteres", texto.Length);
        return new ResultadoOcr(texto, null);
    }

    public static string ParaTextoSimples(string markdown)
    {
        var linhas = markdown
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => !LinhaSeparadoraDeTabela().IsMatch(l))
            .Select(l => l.TrimStart('#', ' ').Replace("**", "").Replace('|', ' '))
            .Select(l => EspacosRepetidos().Replace(l, " ").Trim())
            .Where(l => l.Length > 0);

        return string.Join("\n", linhas);
    }

    private static string TipoMime(string caminho) => Path.GetExtension(caminho).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        _ => "image/jpeg"
    };

    // "| --- | :---: |"
    [GeneratedRegex(@"^\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)*\|?$")]
    private static partial Regex LinhaSeparadoraDeTabela();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex EspacosRepetidos();

    private sealed class RespostaOcr
    {
        [JsonPropertyName("pages")]
        public List<Pagina> Pages { get; set; } = [];
    }

    private sealed class Pagina
    {
        [JsonPropertyName("markdown")]
        public string Markdown { get; set; } = string.Empty;
    }
}
