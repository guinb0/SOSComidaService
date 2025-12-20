namespace SOSComida.Models;

public class Doacao
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = "dinheiro"; // dinheiro, alimento, outro
    public string? Descricao { get; set; }
    public string Status { get; set; } = "confirmada"; // pendente, confirmada, cancelada
    public string? ComprovanteUrl { get; set; }
    public DateTime DataDoacao { get; set; } = DateTime.Now;
    public DateTime? DataConfirmacao { get; set; }

    // Relacionamento com Usuário (doador)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    // Relacionamento com Campanha
    public int CampanhaId { get; set; }
    public Campanha Campanha { get; set; } = null!;
}
