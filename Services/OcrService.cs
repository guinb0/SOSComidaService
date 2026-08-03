using SkiaSharp;
using Tesseract;

namespace SOSComida.Services;

public record ResultadoOcr(string Texto, float Confianca);

public interface IOcrService
{
    ResultadoOcr Ler(string caminhoImagem);
}

/// <summary>
/// OCR local via Tesseract. Sem chave de API e sem custo, mas a qualidade
/// depende muito da foto: cupom térmico amassado, torto ou com sombra sai
/// com erro. Por isso o resultado nunca é tratado como verdade — quem envia
/// a nota revisa os campos antes de valerem (ver <c>NotaFiscal.Revisado</c>).
/// </summary>
public class TesseractOcrService : IOcrService, IDisposable
{
    private readonly TesseractEngine _engine;
    private readonly ILogger<TesseractOcrService> _logger;

    // O engine do Tesseract não é thread-safe e este serviço é singleton.
    // Duas requisições simultâneas quebrariam a leitura, então uma espera a
    // outra. É lento sob carga, mas correto — e o volume aqui é baixo.
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    public TesseractOcrService(IWebHostEnvironment env, ILogger<TesseractOcrService> logger)
    {
        _logger = logger;
        var tessdata = Path.Combine(AppContext.BaseDirectory, "tessdata");

        // "por+eng": nota fiscal brasileira mistura português com siglas e
        // cabeçalhos em inglês, e o modelo de inglês ajuda nos números.
        _engine = new TesseractEngine(tessdata, "por+eng", EngineMode.Default);

        // Cupom fiscal é um bloco único de texto em coluna. Sem isto o
        // Tesseract tenta detectar colunas e embaralha as linhas.
        _engine.DefaultPageSegMode = PageSegMode.SingleBlock;
    }

    public ResultadoOcr Ler(string caminhoImagem)
    {
        // Foto de celular vem grande e colorida; o Tesseract vai muito melhor
        // com a imagem em tons de cinza e reduzida.
        using var bitmap = PreProcessar(caminhoImagem);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);

        _semaforo.Wait();
        try
        {
            using var pix = Pix.LoadFromMemory(data.ToArray());
            using var page = _engine.Process(pix);

            var texto = page.GetText() ?? string.Empty;
            var confianca = page.GetMeanConfidence() * 100f;

            _logger.LogInformation(
                "OCR concluído: {Chars} caracteres, confiança {Confianca:F1}%",
                texto.Length, confianca);

            return new ResultadoOcr(texto.Trim(), confianca);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    /// <summary>
    /// Reduz a imagem para no máximo 2000px no maior lado, converte para
    /// cinza e aplica um limiar. Acima disso o ganho de acurácia some e o
    /// tempo de processamento cresce muito.
    /// </summary>
    private static SKBitmap PreProcessar(string caminho)
    {
        using var original = SKBitmap.Decode(caminho)
            ?? throw new InvalidOperationException("Não foi possível decodificar a imagem.");

        var maiorLado = Math.Max(original.Width, original.Height);
        var escala = maiorLado > 2000 ? 2000f / maiorLado : 1f;

        var largura = (int)(original.Width * escala);
        var altura = (int)(original.Height * escala);

        using var redimensionada = original.Resize(
            new SKImageInfo(largura, altura), new SKSamplingOptions(SKFilterMode.Linear));

        var cinza = new SKBitmap(largura, altura, SKColorType.Gray8, SKAlphaType.Opaque);
        using (var canvas = new SKCanvas(cinza))
        using (var paint = new SKPaint())
        {
            // Matriz que zera a saturação: converte para tons de cinza.
            paint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
                0.299f, 0.587f, 0.114f, 0, 0,
                0.299f, 0.587f, 0.114f, 0, 0,
                0.299f, 0.587f, 0.114f, 0, 0,
                0,      0,      0,      1, 0
            });
            canvas.DrawBitmap(redimensionada ?? original, 0, 0, paint);
        }

        return cinza;
    }

    public void Dispose()
    {
        _engine.Dispose();
        _semaforo.Dispose();
    }
}
