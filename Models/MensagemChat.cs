namespace SOSComida.Models;

public class MensagemChat
{
    public int Id { get; set; }
    
    // Relacionamento com Campanha
    public int CampanhaId { get; set; }
    public Campanha Campanha { get; set; } = null!;
    
    // Relacionamento com Usuário (autor da mensagem)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    
    public string Conteudo { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; } = DateTime.Now;
    public DateTime? DataEdicao { get; set; }
    public bool Deletada { get; set; } = false;
    
    // Para marcar mensagens fixadas pelo moderador
    public bool Fixada { get; set; } = false;
    public int? FixadaPorId { get; set; }
    public Usuario? FixadaPor { get; set; }
}
