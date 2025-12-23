namespace SOSComida.Models;

public class Campanha
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public string? Imagens { get; set; } // JSON array de URLs das imagens
    public string Localizacao { get; set; } = string.Empty;
    public decimal MetaArrecadacao { get; set; }
    public decimal ValorArrecadado { get; set; } = 0;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Status { get; set; } = "pendente"; // pendente, ativa, pausada, finalizada, delegada
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    
    // Relacionamento com Usuário (criador) - nullable para permitir campanhas sem criador definido
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    
    // Instituição responsável (delegação)
    public int? InstituicaoId { get; set; }
    public Usuario? Instituicao { get; set; }
    public string? StatusDelegacao { get; set; } // pendente, aceita, recusada
    public DateTime? DataDelegacao { get; set; }
    
    // Região administrativa da campanha
    public int? RegiaoId { get; set; }
    public RegiaoAdministrativa? Regiao { get; set; }
}
