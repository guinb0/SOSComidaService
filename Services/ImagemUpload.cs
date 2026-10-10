namespace SOSComida.Services;

/// <summary>
/// Validação e gravação de imagens enviadas pelos usuários.
///
/// O tipo é decidido pelos primeiros bytes do arquivo, não pelo
/// <c>Content-Type</c> nem pelo nome — os dois vêm do cliente. Antes, um
/// <c>.html</c> enviado como <c>image/jpeg</c> era gravado com a extensão
/// original e servido pelo <c>UseStaticFiles</c> como página: XSS armazenado.
/// Agora a extensão gravada é sempre a do tipo detectado.
/// </summary>
public static class ImagemUpload
{
    public static string? DetectarExtensao(ReadOnlySpan<byte> cabecalho)
    {
        if (cabecalho.Length >= 3 && cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF)
            return ".jpg";

        if (cabecalho.Length >= 8 && cabecalho[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";

        if (cabecalho.Length >= 6 && (cabecalho[..6].SequenceEqual("GIF87a"u8) || cabecalho[..6].SequenceEqual("GIF89a"u8)))
            return ".gif";

        if (cabecalho.Length >= 12 && cabecalho[..4].SequenceEqual("RIFF"u8) && cabecalho[8..12].SequenceEqual("WEBP"u8))
            return ".webp";

        return null;
    }

    public static async Task<string?> DetectarExtensaoAsync(IFormFile arquivo)
    {
        var buffer = new byte[12];
        await using var stream = arquivo.OpenReadStream();
        var lidos = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false);
        return DetectarExtensao(buffer.AsSpan(0, lidos));
    }

    /// <summary>Grava em wwwroot/uploads/{subpasta} e devolve a URL pública.</summary>
    public static async Task<(string Url, string Caminho)> SalvarAsync(
        IFormFile arquivo, string extensao, string subpasta, string prefixo)
    {
        var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subpasta);
        Directory.CreateDirectory(pasta);

        var nome = $"{prefixo}_{Guid.NewGuid()}{extensao}";
        var caminho = Path.Combine(pasta, nome);

        await using (var stream = new FileStream(caminho, FileMode.CreateNew))
            await arquivo.CopyToAsync(stream);

        return ($"/uploads/{subpasta}/{nome}", caminho);
    }

    /// <summary>
    /// Apaga um arquivo a partir da URL guardada no banco, sem sair de
    /// wwwroot/uploads mesmo que a URL tenha sido adulterada.
    /// </summary>
    public static void Apagar(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;

        var raiz = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"));
        var caminho = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", url.TrimStart('/')));

        if (caminho.StartsWith(raiz + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && File.Exists(caminho))
            File.Delete(caminho);
    }
}
