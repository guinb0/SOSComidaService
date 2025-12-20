namespace SOSComida.Models;

public class Denuncia
{
    public int Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = "pendente"; // pendente, em_analise, procedente, improcedente
    public string? ParecerModerador { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAnalise { get; set; }

    // Tipo do item denunciado (campanha, usuario, pedido_ajuda)
    public string TipoDenunciado { get; set; } = string.Empty;
    public int DenunciadoId { get; set; }

    // Relacionamento com Usuário (denunciante)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    // Moderador que analisou (opcional)
    public int? ModeradorId { get; set; }
    public Usuario? Moderador { get; set; }
}
