namespace SOSComida.Models;

public class AcaoModeracao
{
    public int Id { get; set; }
    public string TipoAcao { get; set; } = string.Empty; // aprovacao, rejeicao, suspensao, banimento, advertencia
    public string Motivo { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime DataAcao { get; set; } = DateTime.Now;

    // Tipo e Id do item moderado (campanha, usuario, pedido_ajuda, denuncia)
    public string TipoItem { get; set; } = string.Empty;
    public int ItemId { get; set; }

    // Moderador que executou a ação
    public int ModeradorId { get; set; }
    public Usuario Moderador { get; set; } = null!;
}
