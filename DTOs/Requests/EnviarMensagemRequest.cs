using System.ComponentModel.DataAnnotations;

namespace SOSComida.DTOs.Requests;

public class EnviarMensagemRequest
{
    [Required(ErrorMessage = "A mensagem está vazia"), MaxLength(2000, ErrorMessage = "Mensagem muito longa")]
    public string Conteudo { get; set; } = string.Empty;
}
