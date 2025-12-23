namespace SOSComida.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string Tipo { get; set; } = "usuario"; // usuario, instituicao, moderador, admin
    public bool SenhaTemporaria { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public string? FotoUrl { get; set; }
    
    // Campos específicos para instituições
    public string? Cnpj { get; set; }
    public int? RegiaoAdministrativaId { get; set; }
    public RegiaoAdministrativa? RegiaoAdministrativa { get; set; }
    public string? NomeInstituicao { get; set; } // Nome fantasia da instituição
    public string? DescricaoInstituicao { get; set; }
    public string StatusAprovacao { get; set; } = "nao_aplicavel"; // nao_aplicavel, pendente, aprovado, rejeitado
    public DateTime? DataAprovacao { get; set; }
    public int? AprovadoPorId { get; set; }
    public Usuario? AprovadoPor { get; set; }
    public string? MotivoRejeicao { get; set; }
}
