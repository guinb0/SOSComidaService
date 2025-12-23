namespace SOSComida.Models;

public class PedidoAjuda
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string Urgencia { get; set; } = "media"; // baixa, media, alta, critica
    public string Status { get; set; } = "pendente"; // pendente, em_andamento, atendido, cancelado
    public int QuantidadePessoas { get; set; } = 1;
    public string? Observacoes { get; set; }
    public string? Telefone { get; set; }
    public string? TipoAjuda { get; set; } // alimentos, higiene, roupas, medicamentos, outros
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataAtendimento { get; set; }

    // Relacionamento com Usuário (solicitante)
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    // Campanha que atendeu (opcional)
    public int? CampanhaId { get; set; }
    public Campanha? Campanha { get; set; }
    
    // Região administrativa
    public int? RegiaoId { get; set; }
    public RegiaoAdministrativa? Regiao { get; set; }
    
    // Usuário que atendeu
    public int? AtendidoPorId { get; set; }
    public Usuario? AtendidoPor { get; set; }
}
