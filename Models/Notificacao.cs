namespace SOSComida.Models;

public class Notificacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Tipo { get; set; } = string.Empty; // delegacao_campanha, advertencia, geral
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public bool Lida { get; set; } = false;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    // Referência opcional para campanhas (delegação)
    public int? CampanhaId { get; set; }
    public Campanha? Campanha { get; set; }
    
    // Status da delegação (pendente, aceita, recusada)
    public string? StatusDelegacao { get; set; }
}
