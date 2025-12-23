namespace SOSComida.DTOs.Requests;

public class RegisterRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
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
