namespace SOSComida.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }

    /// <summary>UsuarioDto, e não a entidade Usuario: esta última carrega o campo Senha.</summary>
    public UsuarioDto Usuario { get; set; } = null!;

    public bool? Requires2FA { get; set; }
    public bool SenhaTemporaria { get; set; } = false;
}
