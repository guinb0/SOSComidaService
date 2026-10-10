using System.ComponentModel.DataAnnotations;

namespace SOSComida.DTOs.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "O nome é obrigatório"), MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório"), EmailAddress(ErrorMessage = "E-mail inválido"), MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string Tipo { get; set; } = "usuario"; // usuario ou instituicao

    // Campos específicos para instituições
    public string? Cnpj { get; set; }
    public int? RegiaoAdministrativaId { get; set; }
    public string? NomeInstituicao { get; set; }
    public string? DescricaoInstituicao { get; set; }
}
