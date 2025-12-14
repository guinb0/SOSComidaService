namespace SOSComida.DTOs.Requests;

public class CreateCampanhaDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public string? Localizacao { get; set; }
    public decimal MetaArrecadacao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}
