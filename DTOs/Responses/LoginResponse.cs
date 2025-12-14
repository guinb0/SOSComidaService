using SOSComida.Models;

namespace SOSComida.DTOs.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public bool? Requires2FA { get; set; }
}
