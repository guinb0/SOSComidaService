namespace SOSComida.Models;

public class ParticipanteCampanha
{
    public int Id { get; set; }
    
    // Relacionamento com Campanha
    public int CampanhaId { get; set; }
    public Campanha Campanha { get; set; } = null!;
    
    // Relacionamento com Usuário (participante)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    
    public string Tipo { get; set; } = "voluntario"; // voluntario, doador, beneficiario
    public string Status { get; set; } = "ativo"; // ativo, removido, suspenso
    public DateTime DataEntrada { get; set; } = DateTime.Now;
    public DateTime? DataSaida { get; set; }
    public string? MotivoSaida { get; set; }
    
    // Quem removeu (se foi removido por moderador)
    public int? RemovidoPorId { get; set; }
    public Usuario? RemovidoPor { get; set; }
}
