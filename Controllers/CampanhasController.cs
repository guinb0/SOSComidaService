using Microsoft.AspNetCore.Mvc;
using SOSComida.DTOs.Responses;
using SOSComida.Models;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/campanhas")]
public class CampanhasController : ControllerBase
{
    private readonly string _campanhasPath;

    public CampanhasController(IWebHostEnvironment env)
    {
        _campanhasPath = Path.Combine(env.ContentRootPath, "Data", "campanhas.txt");
    }

    private List<Campanha> LerCampanhas()
    {
        if (!System.IO.File.Exists(_campanhasPath))
            return new List<Campanha>();

        var linhas = System.IO.File.ReadAllLines(_campanhasPath);
        var campanhas = new List<Campanha>();

        foreach (var linha in linhas)
        {
            var partes = linha.Split('|');
            if (partes.Length >= 12)
            {
                campanhas.Add(new Campanha
                {
                    Id = int.Parse(partes[0]),
                    Titulo = partes[1],
                    Descricao = partes[2],
                    ImagemUrl = partes[3],
                    Localizacao = partes[4],
                    MetaArrecadacao = decimal.Parse(partes[5]),
                    ValorArrecadado = decimal.Parse(partes[6]),
                    DataInicio = DateTime.Parse(partes[7]),
                    DataFim = DateTime.Parse(partes[8]),
                    Status = partes[9],
                    Ativa = bool.Parse(partes[10]),
                    UsuarioId = int.Parse(partes[11])
                });
            }
        }

        return campanhas;
    }

    // GET: api/campanhas
    [HttpGet]
    public ActionResult<IEnumerable<CampanhaDto>> GetAll()
    {
        var campanhas = LerCampanhas()
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .ToList();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa
        }).ToList();
        
        return Ok(new { data = campanhasDto, total = campanhasDto.Count });
    }

    // GET: api/campanhas/principais
    [HttpGet("principais")]
    public ActionResult<IEnumerable<CampanhaDto>> GetPrincipais()
    {
        var campanhas = LerCampanhas()
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .Take(3)
            .ToList();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa
        }).ToList();
        
        return Ok(new { data = campanhasDto });
    }

    // GET: api/campanhas/5
    [HttpGet("{id}")]
    public ActionResult<CampanhaDto> GetById(int id)
    {
        var campanha = LerCampanhas().FirstOrDefault(c => c.Id == id);

        if (campanha == null)
            return NotFound(new { message = "Campanha não encontrada" });

        return Ok(new CampanhaDto
        {
            Id = campanha.Id,
            Titulo = campanha.Titulo,
            Descricao = campanha.Descricao,
            ImagemUrl = campanha.ImagemUrl,
            Localizacao = campanha.Localizacao,
            MetaArrecadacao = campanha.MetaArrecadacao,
            ValorArrecadado = campanha.ValorArrecadado,
            Progresso = campanha.MetaArrecadacao > 0 ? (int)((campanha.ValorArrecadado / campanha.MetaArrecadacao) * 100) : 0,
            DataInicio = campanha.DataInicio,
            DataFim = campanha.DataFim,
            Status = campanha.Status,
            Ativa = campanha.Ativa
        });
    }

    // GET: api/campanhas/usuario/5
    [HttpGet("usuario/{usuarioId}")]
    public ActionResult<IEnumerable<CampanhaDto>> GetByUsuario(int usuarioId)
    {
        var campanhas = LerCampanhas()
            .Where(c => c.UsuarioId == usuarioId && c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .ToList();

        var campanhasDto = campanhas.Select(c => new CampanhaDto
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemUrl,
            Localizacao = c.Localizacao,
            MetaArrecadacao = c.MetaArrecadacao,
            ValorArrecadado = c.ValorArrecadado,
            Progresso = c.MetaArrecadacao > 0 ? (int)((c.ValorArrecadado / c.MetaArrecadacao) * 100) : 0,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            Status = c.Status,
            Ativa = c.Ativa
        }).ToList();
        
        return Ok(new { data = campanhasDto, total = campanhasDto.Count });
    }
}
