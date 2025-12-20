namespace SOSComida.DTOs.Responses;

public class AvisoCampanhaDto
{
    public int Id { get; set; }
    public int CampanhaId { get; set; }
    public string TituloCampanha { get; set; } = string.Empty;
    public int ModeradorId { get; set; }
    public string NomeModerador { get; set; } = string.Empty;
    public int? DestinatarioId { get; set; }
    public string? NomeDestinatario { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
    public bool Lido { get; set; }
    public DateTime? DataLeitura { get; set; }
}
