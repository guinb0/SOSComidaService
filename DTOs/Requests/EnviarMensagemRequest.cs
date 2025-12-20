namespace SOSComida.DTOs.Requests;

public class EnviarMensagemRequest
{
    public int CampanhaId { get; set; }
    public int UsuarioId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
}
