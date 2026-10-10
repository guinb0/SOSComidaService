using SOSComida.Models;
using SOSComida.Seguranca;
using SOSComida.Services;

namespace SOSComida.Tests;

public class SenhaServiceTests
{
    [Fact]
    public void Hash_confere_com_a_senha_certa_e_nao_com_a_errada()
    {
        var hash = SenhaService.Hash("teste123");

        Assert.True(SenhaService.EhHash(hash));
        Assert.True(SenhaService.Confere(hash, "teste123"));
        Assert.False(SenhaService.Confere(hash, "teste124"));
    }

    [Fact]
    public void Senha_em_texto_puro_nao_e_aceita_como_hash()
    {
        Assert.False(SenhaService.EhHash("teste123"));
        Assert.False(SenhaService.Confere("teste123", "teste123"));
    }
}

public class ImagemUploadTests
{
    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png")]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, ".gif")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50 }, ".webp")]
    public void Detecta_o_tipo_pelos_bytes(byte[] cabecalho, string esperado)
    {
        Assert.Equal(esperado, ImagemUpload.DetectarExtensao(cabecalho));
    }

    [Fact]
    public void Recusa_html_mesmo_com_nome_de_imagem()
    {
        Assert.Null(ImagemUpload.DetectarExtensao("<html><script>"u8));
    }
}

public class PermissoesCampanhaTests
{
    private static Campanha Campanha(int? criador = 1, int? instituicao = null, string? delegacao = null) =>
        new() { UsuarioId = criador, InstituicaoId = instituicao, StatusDelegacao = delegacao };

    [Fact]
    public void Criador_responde_pela_campanha() =>
        Assert.True(PermissoesCampanha.PodeResponderPor(Campanha(criador: 7), 7, ehModerador: false));

    [Fact]
    public void Outro_usuario_nao_responde() =>
        Assert.False(PermissoesCampanha.PodeResponderPor(Campanha(criador: 7), 8, ehModerador: false));

    [Fact]
    public void Moderador_responde_por_qualquer_campanha() =>
        Assert.True(PermissoesCampanha.PodeResponderPor(Campanha(criador: 7), 8, ehModerador: true));

    [Fact]
    public void Instituicao_so_responde_depois_de_aceitar_a_delegacao()
    {
        Assert.False(PermissoesCampanha.PodeResponderPor(Campanha(instituicao: 9, delegacao: "pendente"), 9, false));
        Assert.True(PermissoesCampanha.PodeResponderPor(Campanha(instituicao: 9, delegacao: "aceita"), 9, false));
    }
}
