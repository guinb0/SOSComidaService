using System.ComponentModel.DataAnnotations;

namespace SOSComida.DTOs.Requests;

public class EnviarAvisoRequest
{
    public int? DestinatarioId { get; set; } // null = aviso para todos

    [Required(ErrorMessage = "Informe o título"), MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva a mensagem"), MaxLength(2000)]
    public string Mensagem { get; set; } = string.Empty;

    [RegularExpression("informativo|advertencia|urgente", ErrorMessage = "Tipo de aviso inválido")]
    public string Tipo { get; set; } = "informativo";
}
