namespace SOSComida.Models;

public class Campanha
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public string Localizacao { get; set; } = string.Empty;
    public decimal MetaArrecadacao { get; set; }
    public decimal ValorArrecadado { get; set; } = 0;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Status { get; set; } = "ativa"; // ativa, pausada, finalizada
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }
    
    // Relacionamento com Usuário (criador)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
