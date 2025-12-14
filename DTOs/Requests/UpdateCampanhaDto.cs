namespace SOSComida.DTOs.Requests;

public class UpdateCampanhaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public decimal MetaArrecadacao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Status { get; set; } = "ativa";
    public bool Ativa { get; set; }
}
