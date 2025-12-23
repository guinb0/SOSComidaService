namespace SOSComida.Models;

public class RegiaoAdministrativa
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty; // Ex: "DF-ASA", "DF-TAG", "SP-ZL"
    public string? Estado { get; set; }
    public string? Cidade { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    // Navegação para moderadores
    public ICollection<ModeradorRegiao> ModeradorRegioes { get; set; } = new List<ModeradorRegiao>();
}
