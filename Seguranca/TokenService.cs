using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SOSComida.Models;

namespace SOSComida.Seguranca;

public class JwtSettings
{
    public string Chave { get; set; } = string.Empty;
    public string Emissor { get; set; } = "soscomida-api";
    public string Audiencia { get; set; } = "soscomida-web";
    public int ExpiracaoHoras { get; set; } = 12;

    /// <summary>
    /// Chave que vem no appsettings.Development.json. Está no git, então
    /// subir em produção com ela é o mesmo que não ter chave nenhuma.
    /// </summary>
    public const string ChaveDeDesenvolvimento =
        "chave-de-desenvolvimento-sos-comida-NAO-USAR-EM-PRODUCAO";

    public SymmetricSecurityKey ChaveDeAssinatura() => new(Encoding.UTF8.GetBytes(Chave));

    public void Validar(bool desenvolvimento)
    {
        if (Encoding.UTF8.GetByteCount(Chave) < 32)
            throw new InvalidOperationException(
                "Jwt:Chave ausente ou curta (mínimo 32 bytes). Defina a variável de ambiente Jwt__Chave.");

        if (!desenvolvimento && Chave == ChaveDeDesenvolvimento)
            throw new InvalidOperationException(
                "Jwt:Chave de desenvolvimento em uso fora do ambiente Development. Defina Jwt__Chave.");
    }
}

public class TokenService
{
    public const string ClaimId = "sub";
    public const string ClaimNome = "name";
    public const string ClaimPapel = "role";

    private readonly JwtSettings _settings;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenService(JwtSettings settings) => _settings = settings;

    public string Gerar(Usuario usuario) => _handler.CreateToken(new SecurityTokenDescriptor
    {
        Issuer = _settings.Emissor,
        Audience = _settings.Audiencia,
        Expires = DateTime.UtcNow.AddHours(_settings.ExpiracaoHoras),
        SigningCredentials = new SigningCredentials(_settings.ChaveDeAssinatura(), SecurityAlgorithms.HmacSha256),
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimId, usuario.Id.ToString()),
            new Claim(ClaimNome, usuario.Nome),
            new Claim(ClaimPapel, usuario.Tipo),
            new Claim("email", usuario.Email)
        })
    });
}
