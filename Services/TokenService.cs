using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SOSComida.Models;

namespace SOSComida.Services;

/// <summary>
/// Emite o JWT do login. Antes, o "token" era a string
/// `mock-jwt-token-{id}-{guid}` — não era assinado, não expirava e não provava
/// nada: qualquer texto no lugar dele funcionava igual, porque ninguém validava.
/// </summary>
public interface ITokenService
{
    string Gerar(Usuario usuario);
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _cfg;

    public TokenService(JwtSettings cfg) => _cfg = cfg;

    public string Gerar(Usuario usuario)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Secret));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("nome", usuario.Nome),
            // O papel vira ClaimTypes.Role para que [Authorize(Roles = "...")] funcione.
            new(ClaimTypes.Role, usuario.Tipo),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _cfg.Issuer,
            audience: _cfg.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(_cfg.HorasDeValidade),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class JwtSettings
{
    /// <summary>
    /// Chave de assinatura. NUNCA versionar a de produção: defina por variável de
    /// ambiente (`Jwt__Secret`). Mínimo de 32 bytes para HMAC-SHA256.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "soscomida";
    public string Audience { get; set; } = "soscomida-app";
    public int HorasDeValidade { get; set; } = 12;
}
