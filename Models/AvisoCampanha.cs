namespace SOSComida.Models;

public class AvisoCampanha
{
    public int Id { get; set; }
    
    // Relacionamento com Campanha
    public int CampanhaId { get; set; }
    public Campanha Campanha { get; set; } = null!;
    
    // Moderador que enviou o aviso
    public int ModeradorId { get; set; }
    public Usuario Moderador { get; set; } = null!;
    
    // Se for aviso para um usuário específico (null = todos da campanha)
    public int? DestinatarioId { get; set; }
    public Usuario? Destinatario { get; set; }
    
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = "informativo"; // informativo, advertencia, urgente
    public DateTime DataEnvio { get; set; } = DateTime.Now;
    public bool Lido { get; set; } = false;
    public DateTime? DataLeitura { get; set; }
}
