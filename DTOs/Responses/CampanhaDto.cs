namespace SOSComida.DTOs.Responses;

public class CampanhaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public string Localizacao { get; set; } = string.Empty;
    public decimal MetaArrecadacao { get; set; }
    public decimal ValorArrecadado { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public int Progresso { get; set; }
    public decimal PercentualArrecadado => MetaArrecadacao > 0 
        ? (ValorArrecadado / MetaArrecadacao) * 100 
        : 0;
}
