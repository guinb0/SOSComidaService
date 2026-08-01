using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;

namespace SOSComida.Services;

/// <summary>
/// Emite, valida e rotaciona refresh tokens.
///
/// O token cru (256 bits aleatórios) só sai daqui uma vez, na criação. O banco
/// guarda apenas o SHA-256 dele — como já é alta entropia, SHA-256 basta (não
/// precisa de PBKDF2 como a senha, que é de baixa entropia).
///
/// Rotação com detecção de reuso: cada refresh revoga o token usado e emite um
/// novo. Se um token JÁ revogado por rotação for reapresentado, alguém está com
/// uma cópia roubada — revogamos toda a cadeia de tokens do usuário.
/// </summary>
public interface IRefreshTokenService
{
    /// <returns>o token cru, para devolver ao cliente.</returns>
    Task<string> EmitirAsync(int usuarioId);

    /// <summary>Rotaciona: valida o token cru, revoga-o e emite outro. Null se inválido.</summary>
    Task<(string novoToken, Usuario usuario)?> RotacionarAsync(string tokenCru);

    /// <summary>Revoga o token (logout). Silencioso se não existir.</summary>
    Task RevogarAsync(string tokenCru);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private static readonly TimeSpan Validade = TimeSpan.FromDays(30);

    public RefreshTokenService(ApplicationDbContext context) => _context = context;

    private static string Hash(string tokenCru)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(tokenCru));
        return Convert.ToBase64String(bytes);
    }

    private static string GerarCru() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public async Task<string> EmitirAsync(int usuarioId)
    {
        var cru = GerarCru();
        _context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = Hash(cru),
            UsuarioId = usuarioId,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = DateTime.UtcNow.Add(Validade),
        });
        await _context.SaveChangesAsync();
        return cru;
    }

    public async Task<(string novoToken, Usuario usuario)?> RotacionarAsync(string tokenCru)
    {
        var hash = Hash(tokenCru);
        var registro = await _context.RefreshTokens
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (registro is null || registro.Usuario is null)
            return null;

        // Reuso de um token já rotacionado: comprometimento. Derruba toda a cadeia
        // do usuário, forçando novo login em qualquer dispositivo.
        if (registro.RevogadoEm is not null)
        {
            await RevogarTodosDoUsuarioAsync(registro.UsuarioId);
            return null;
        }

        if (DateTime.UtcNow >= registro.ExpiraEm)
            return null;

        var novoCru = GerarCru();
        var novoHash = Hash(novoCru);

        registro.RevogadoEm = DateTime.UtcNow;
        registro.SubstituidoPor = novoHash;

        _context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = novoHash,
            UsuarioId = registro.UsuarioId,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = DateTime.UtcNow.Add(Validade),
        });

        await _context.SaveChangesAsync();
        return (novoCru, registro.Usuario);
    }

    public async Task RevogarAsync(string tokenCru)
    {
        var hash = Hash(tokenCru);
        var registro = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevogadoEm == null);

        if (registro is null) return;

        registro.RevogadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task RevogarTodosDoUsuarioAsync(int usuarioId)
    {
        await _context.RefreshTokens
            .Where(r => r.UsuarioId == usuarioId && r.RevogadoEm == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.RevogadoEm, DateTime.UtcNow));
    }
}
