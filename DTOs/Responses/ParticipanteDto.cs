namespace SOSComida.DTOs.Responses;

public class ParticipanteDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public DateTime? DataSaida { get; set; }
    public string? MotivoSaida { get; set; }
}
