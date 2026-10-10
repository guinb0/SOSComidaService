using Microsoft.AspNetCore.Identity;
using SOSComida.Models;

namespace SOSComida.Seguranca;

/// <summary>
/// Hash de senha com o <see cref="PasswordHasher{TUser}"/> do ASP.NET Core
/// (PBKDF2 com salt, formato v3). A coluna continua se chamando <c>Senha</c>
/// para não exigir migração de schema, mas guarda só o hash.
/// </summary>
public static class SenhaService
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    public static string Hash(string senha) => Hasher.HashPassword(null!, senha);

    public static bool Confere(string? hash, string senha)
    {
        if (string.IsNullOrEmpty(hash) || !EhHash(hash)) return false;
        try
        {
            return Hasher.VerifyHashedPassword(null!, hash, senha) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// O formato v3 é base64 de um payload que começa com o byte 0x01 e o
    /// PRF em big-endian, então todo hash começa com "AQAAAA". Senha em texto
    /// puro gravada antes do hash não começa assim — é assim que o arranque
    /// encontra as senhas antigas para converter.
    /// </summary>
    public static bool EhHash(string valor) => valor.StartsWith("AQAAAA", StringComparison.Ordinal);
}
