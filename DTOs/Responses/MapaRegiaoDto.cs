namespace SOSComida.DTOs.Responses;

/// <summary>
/// Uma Região Administrativa do DF com os totais de campanhas agregados.
/// O endpoint devolve as 33 RAs sempre, inclusive as zeradas — o mapa
/// coroplético precisa pintar a região vazia com o tom mais claro da escala.
/// </summary>
public class MapaRegiaoDto
{
    public int RegiaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;

    public int TotalCampanhas { get; set; }
    public int CampanhasAtivas { get; set; }
    public decimal ValorArrecadado { get; set; }
    public decimal MetaTotal { get; set; }
}
