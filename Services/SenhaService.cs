using System.Security.Cryptography;

namespace SOSComida.Services;

/// <summary>
/// Hash de senha com PBKDF2-HMAC-SHA256 (RFC 2898), embutido no .NET — sem
/// dependência externa.
///
/// Antes, a senha era guardada e comparada em TEXTO PURO
/// (`u.Senha == request.Senha`): quem lesse o banco lia as senhas de todo mundo.
///
/// Formato guardado:  pbkdf2$&lt;iteracoes&gt;$&lt;salt-b64&gt;$&lt;hash-b64&gt;
/// O formato carrega as iterações para que o custo possa ser elevado no futuro
/// sem invalidar os hashes já existentes.
/// </summary>
public interface ISenhaService
{
    string Hash(string senha);

    /// <summary>Confere a senha. Aceita hashes antigos em texto puro (ver <see cref="PrecisaRehash"/>).</summary>
    bool Confere(string senha, string armazenado);

    /// <summary>True quando o valor guardado não está no formato atual e deve ser regravado no próximo login.</summary>
    bool PrecisaRehash(string armazenado);
}

public class SenhaService : ISenhaService
{
    private const int Iteracoes = 210_000; // recomendação OWASP para PBKDF2-SHA256
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const string Prefixo = "pbkdf2";

    public string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Derivar(senha, salt, Iteracoes);
        return $"{Prefixo}${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Confere(string senha, string armazenado)
    {
        if (string.IsNullOrEmpty(armazenado)) return false;

        var partes = armazenado.Split('$');

        // Senha antiga, em texto puro. Aceita uma vez para não trancar ninguém
        // para fora; o login regrava no formato novo (ver PrecisaRehash).
        if (partes.Length != 4 || partes[0] != Prefixo)
        {
            return CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(senha),
                System.Text.Encoding.UTF8.GetBytes(armazenado));
        }

        if (!int.TryParse(partes[1], out var iteracoes)) return false;

        byte[] salt, esperado;
        try
        {
            salt = Convert.FromBase64String(partes[2]);
            esperado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Derivar(senha, salt, iteracoes, esperado.Length);

        // Comparação em tempo constante: um `==` vazaria o tamanho do prefixo
        // correto pelo tempo de resposta.
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }

    public bool PrecisaRehash(string armazenado)
    {
        var partes = armazenado.Split('$');
        if (partes.Length != 4 || partes[0] != Prefixo) return true;
        return !int.TryParse(partes[1], out var it) || it < Iteracoes;
    }

    private static byte[] Derivar(string senha, byte[] salt, int iteracoes, int tamanho = TamanhoHash)
        => Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, tamanho);
}
