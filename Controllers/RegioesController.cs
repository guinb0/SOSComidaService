using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegioesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RegioesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/regioes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegiaoAdministrativa>>> GetRegioes()
    {
        var regioes = await _context.RegioesAdministrativas
            .Where(r => r.Ativa)
            .OrderBy(r => r.Nome)
            .ToListAsync();

        return Ok(regioes);
    }

    // GET: api/regioes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<RegiaoAdministrativa>> GetRegiao(int id)
    {
        var regiao = await _context.RegioesAdministrativas.FindAsync(id);

        if (regiao == null)
        {
            return NotFound(new { message = "Região não encontrada" });
        }

        return Ok(regiao);
    }

    // POST: api/regioes
    [HttpPost]
    public async Task<ActionResult<RegiaoAdministrativa>> CreateRegiao([FromBody] CreateRegiaoRequest request)
    {
        // Verificar se já existe uma região com a mesma sigla
        var existente = await _context.RegioesAdministrativas
            .FirstOrDefaultAsync(r => r.Sigla == request.Sigla);

        if (existente != null)
        {
            return BadRequest(new { message = "Já existe uma região com essa sigla" });
        }

        var regiao = new RegiaoAdministrativa
        {
            Nome = request.Nome,
            Sigla = request.Sigla,
            Estado = request.Estado,
            Cidade = request.Cidade,
            Ativa = true,
            DataCriacao = DateTime.UtcNow
        };

        _context.RegioesAdministrativas.Add(regiao);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRegiao), new { id = regiao.Id }, regiao);
    }

    // PUT: api/regioes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRegiao(int id, [FromBody] UpdateRegiaoRequest request)
    {
        var regiao = await _context.RegioesAdministrativas.FindAsync(id);

        if (regiao == null)
        {
            return NotFound(new { message = "Região não encontrada" });
        }

        regiao.Nome = request.Nome ?? regiao.Nome;
        regiao.Sigla = request.Sigla ?? regiao.Sigla;
        regiao.Estado = request.Estado ?? regiao.Estado;
        regiao.Cidade = request.Cidade ?? regiao.Cidade;

        await _context.SaveChangesAsync();

        return Ok(regiao);
    }

    // DELETE: api/regioes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRegiao(int id)
    {
        var regiao = await _context.RegioesAdministrativas.FindAsync(id);

        if (regiao == null)
        {
            return NotFound(new { message = "Região não encontrada" });
        }

        // Soft delete - desativar ao invés de remover
        regiao.Ativa = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Região desativada com sucesso" });
    }

    // GET: api/regioes/moderador/{moderadorId}
    [HttpGet("moderador/{moderadorId}")]
    public async Task<ActionResult<IEnumerable<RegiaoAdministrativa>>> GetRegioesModerador(int moderadorId)
    {
        var regioes = await _context.ModeradorRegioes
            .Where(mr => mr.ModeradorId == moderadorId && mr.Ativo)
            .Include(mr => mr.Regiao)
            .Select(mr => mr.Regiao)
            .ToListAsync();

        return Ok(regioes);
    }

    // POST: api/regioes/moderador/{moderadorId}/atribuir
    [HttpPost("moderador/{moderadorId}/atribuir")]
    public async Task<IActionResult> AtribuirRegioes(int moderadorId, [FromBody] AtribuirRegioesRequest request)
    {
        var moderador = await _context.Usuarios.FindAsync(moderadorId);

        if (moderador == null)
        {
            return NotFound(new { message = "Moderador não encontrado" });
        }

        if (moderador.Tipo != "moderador" && moderador.Tipo != "admin")
        {
            return BadRequest(new { message = "Usuário não é um moderador" });
        }

        // Remover atribuições antigas
        var atribuicoesAntigas = await _context.ModeradorRegioes
            .Where(mr => mr.ModeradorId == moderadorId)
            .ToListAsync();

        _context.ModeradorRegioes.RemoveRange(atribuicoesAntigas);

        // Adicionar novas atribuições
        foreach (var regiaoId in request.RegioesIds)
        {
            var regiao = await _context.RegioesAdministrativas.FindAsync(regiaoId);
            if (regiao != null && regiao.Ativa)
            {
                _context.ModeradorRegioes.Add(new ModeradorRegiao
                {
                    ModeradorId = moderadorId,
                    RegiaoId = regiaoId,
                    DataAtribuicao = DateTime.UtcNow,
                    Ativo = true
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Regiões atribuídas com sucesso" });
    }
}

public class CreateRegiaoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string? Estado { get; set; }
    public string? Cidade { get; set; }
}

public class UpdateRegiaoRequest
{
    public string? Nome { get; set; }
    public string? Sigla { get; set; }
    public string? Estado { get; set; }
    public string? Cidade { get; set; }
}

public class AtribuirRegioesRequest
{
    public List<int> RegioesIds { get; set; } = new();
}
