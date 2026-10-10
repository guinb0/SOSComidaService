using System.Security.Claims;

namespace SOSComida.Seguranca;

/// <summary>
/// Quem está chamando vem sempre do token, nunca do corpo ou da query.
/// Antes a API confiava no <c>usuarioId</c>/<c>moderadorId</c> enviado pelo
/// cliente, e qualquer um podia agir em nome de qualquer outro.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static int Id(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(TokenService.ClaimId);
        return int.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Requisição sem usuário autenticado.");
    }

    public static int? IdOuNulo(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(TokenService.ClaimId), out var id) ? id : null;

    public static bool EhModerador(this ClaimsPrincipal user) =>
        user.IsInRole(TiposUsuario.Moderador) || user.IsInRole(TiposUsuario.Admin);

    public static bool EhAdmin(this ClaimsPrincipal user) => user.IsInRole(TiposUsuario.Admin);
}
