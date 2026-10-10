using System.ComponentModel.DataAnnotations;

namespace SOSComida.DTOs.Requests;

public class UpdateCampanhaDto
{
    [Required(ErrorMessage = "Título é obrigatório")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;

    public string? ImagemUrl { get; set; }
    public string? Localizacao { get; set; }
    public decimal MetaArrecadacao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    /// <summary>ativa, pausada ou finalizada. Nulo mantém o status atual.</summary>
    public string? Status { get; set; }
}
