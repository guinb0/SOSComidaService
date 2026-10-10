using System.ComponentModel.DataAnnotations;

namespace SOSComida.DTOs.Requests;

public class AlterarSenhaRequest
{
    [Required(ErrorMessage = "Informe a senha atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;
}
