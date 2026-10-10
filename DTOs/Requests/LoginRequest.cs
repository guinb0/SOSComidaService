using System.ComponentModel.DataAnnotations;

namespace SOSComida.DTOs.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Informe o e-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha")]
    public string Senha { get; set; } = string.Empty;

    public string? RecaptchaToken { get; set; }
}
