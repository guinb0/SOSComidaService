namespace SOSComida.DTOs.Requests;

public class RemoverParticipanteRequest
{
    public int CampanhaId { get; set; }
    public int UsuarioId { get; set; }
    public int ModeradorId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
