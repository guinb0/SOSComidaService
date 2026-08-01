namespace SOSComida.Models;

/// <summary>
/// Refresh token: prova de longa duração que permite renovar o access token (JWT)
/// sem pedir a senha de novo. O access token expira em 12h; este vale por semanas.
///
/// O que fica guardado é o HASH do token (SHA-256), nunca o valor cru — se o banco
/// vazar, os tokens não são utilizáveis. O valor cru só existe na resposta HTTP,
/// no caminho até o cliente.
///
/// Rotação: cada uso invalida o token e emite um novo (RevogadoEm + SubstituidoPor).
/// Se um token já rotacionado for reapresentado, é sinal de roubo — ver o serviço.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>SHA-256 (base64) do token cru. Único.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime ExpiraEm { get; set; }

    /// <summary>Preenchido quando o token é rotacionado ou revogado (logout).</summary>
    public DateTime? RevogadoEm { get; set; }

    /// <summary>Hash do token que substituiu este, na rotação. Usado para detectar reuso.</summary>
    public string? SubstituidoPor { get; set; }

    public bool Ativo => RevogadoEm is null && DateTime.UtcNow < ExpiraEm;
}
