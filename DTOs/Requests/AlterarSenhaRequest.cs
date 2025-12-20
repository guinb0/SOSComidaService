namespace SOSComida.DTOs.Requests;

public class AlterarSenhaRequest
{
    public int UsuarioId { get; set; }
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
