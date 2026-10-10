using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SOSComida.Services;

namespace SOSComida.Tests;

public class MistralOcrServiceTests
{
    private sealed class HandlerFalso(HttpStatusCode status, string corpo) : HttpMessageHandler
    {
        public HttpRequestMessage? Requisicao { get; private set; }
        public string? CorpoEnviado { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requisicao = request;
            CorpoEnviado = request.Content == null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(corpo, Encoding.UTF8, "application/json") };
        }
    }

    private static async Task<string> ImagemTemporariaAsync()
    {
        var caminho = Path.Combine(Path.GetTempPath(), $"nota-{Guid.NewGuid()}.png");
        await File.WriteAllBytesAsync(caminho, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        return caminho;
    }

    private static MistralOcrService Servico(HandlerFalso handler) =>
        new(new HttpClient(handler), new OcrSettings { ChaveApi = "chave-teste" }, NullLogger<MistralOcrService>.Instance);

    [Fact]
    public async Task Envia_a_imagem_como_data_url_com_a_chave()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, """{"pages":[{"index":0,"markdown":"TOTAL 10,00"}]}""");
        var caminho = await ImagemTemporariaAsync();

        await Servico(handler).LerAsync(caminho);

        Assert.Equal("https://api.mistral.ai/v1/ocr", handler.Requisicao!.RequestUri!.ToString());
        Assert.Equal("Bearer chave-teste", handler.Requisicao.Headers.Authorization!.ToString());
        var corpo = JsonDocument.Parse(handler.CorpoEnviado!).RootElement;
        Assert.Equal("mistral-ocr-latest", corpo.GetProperty("model").GetString());
        Assert.StartsWith("data:image/png;base64,", corpo.GetProperty("document").GetProperty("image_url").GetString());
        File.Delete(caminho);
    }

    [Fact]
    public async Task Markdown_vira_texto_que_o_extrator_entende()
    {
        var markdown = string.Join('\n',
            "# MERCADO BOM PRECO LTDA",
            "CNPJ: 11.222.333/0001-81",
            "",
            "| Item | Valor |",
            "| --- | ---: |",
            "| ARROZ | 51,80 |",
            "| **VALOR TOTAL R$** | **77,30** |");
        var json = JsonSerializer.Serialize(new { pages = new[] { new { index = 0, markdown } } });
        var handler = new HandlerFalso(HttpStatusCode.OK, json);
        var caminho = await ImagemTemporariaAsync();

        var resultado = await Servico(handler).LerAsync(caminho);
        var campos = ExtratorNotaFiscal.Extrair(resultado.Texto);

        Assert.Null(resultado.Confianca);
        Assert.DoesNotContain("|", resultado.Texto);
        Assert.DoesNotContain("---", resultado.Texto);
        Assert.Equal(77.30m, campos.ValorTotal);
        Assert.NotNull(campos.CnpjEmitente);
        File.Delete(caminho);
    }

    [Fact]
    public async Task Erro_da_api_vira_excecao()
    {
        var handler = new HandlerFalso(HttpStatusCode.Unauthorized, """{"message":"Unauthorized"}""");
        var caminho = await ImagemTemporariaAsync();

        await Assert.ThrowsAsync<HttpRequestException>(() => Servico(handler).LerAsync(caminho));
        File.Delete(caminho);
    }

    [Theory]
    [InlineData("", "", false)]
    [InlineData("", "sk-123", true)]
    [InlineData("tesseract", "sk-123", false)]
    [InlineData("Mistral", "sk-123", true)]
    public void Escolhe_o_provedor_pela_configuracao(string provedor, string chave, bool usaMistral) =>
        Assert.Equal(usaMistral, new OcrSettings { Provedor = provedor, ChaveApi = chave }.UsaMistral);
}
