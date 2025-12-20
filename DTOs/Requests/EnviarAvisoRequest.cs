namespace SOSComida.DTOs.Requests;

public class EnviarAvisoRequest
{
    public int CampanhaId { get; set; }
    public int ModeradorId { get; set; }
    public int? DestinatarioId { get; set; } // null = aviso para todos
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = "informativo"; // informativo, advertencia, urgente
}
