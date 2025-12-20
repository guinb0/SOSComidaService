namespace SOSComida.DTOs.Responses;

public class MensagemChatDto
{
    public int Id { get; set; }
    public int CampanhaId { get; set; }
    public int UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
    public DateTime? DataEdicao { get; set; }
    public bool Fixada { get; set; }
    public string? FixadaPorNome { get; set; }
}
