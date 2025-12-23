namespace SOSComida.Models;

// Tabela de relacionamento muitos-para-muitos entre Moderador e Região
public class ModeradorRegiao
{
    public int Id { get; set; }
    
    public int ModeradorId { get; set; }
    public Usuario Moderador { get; set; } = null!;
    
    public int RegiaoId { get; set; }
    public RegiaoAdministrativa Regiao { get; set; } = null!;
    
    public DateTime DataAtribuicao { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
}
