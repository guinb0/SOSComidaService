using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.Seguranca;
using SOSComida.Services;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/instituicoes")]
[Authorize(Policy = Politicas.Moderacao)]
public class InstituicoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstituicoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/instituicoes - Listar todas as instituições
    [HttpGet]
    public async Task<IActionResult> GetInstituicoes([FromQuery] string? status = null)
    {
        var query = _context.Usuarios
            .Where(u => u.Tipo == TiposUsuario.Instituicao)
            .Include(u => u.RegiaoAdministrativa)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(u => u.StatusAprovacao == status);
        }

        var instituicoes = await query
            .OrderByDescending(u => u.DataCriacao)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Endereco,
                u.Cidade,
                u.Cnpj,
                u.NomeInstituicao,
                u.DescricaoInstituicao,
                u.StatusAprovacao,
                u.DataCriacao,
                u.DataAprovacao,
                u.MotivoRejeicao,
                RegiaoAdministrativa = u.RegiaoAdministrativa != null ? new
                {
                    u.RegiaoAdministrativa.Id,
                    u.RegiaoAdministrativa.Nome,
                    u.RegiaoAdministrativa.Sigla
                } : null
            })
            .ToListAsync();

        return Ok(new { data = instituicoes, total = instituicoes.Count });
    }

    // GET: api/instituicoes/pendentes - Listar instituições pendentes de aprovação
    [HttpGet("pendentes")]
    public async Task<IActionResult> GetInstituicoesPendentes()
    {
        var instituicoes = await _context.Usuarios
            .Where(u => u.Tipo == TiposUsuario.Instituicao && u.StatusAprovacao == StatusAprovacoes.Pendente)
            .Include(u => u.RegiaoAdministrativa)
            .OrderBy(u => u.DataCriacao)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Endereco,
                u.Cidade,
                u.Cnpj,
                u.NomeInstituicao,
                u.DescricaoInstituicao,
                u.DataCriacao,
                EmailCorporativo = !u.Email.Contains("@gmail.") && 
                                   !u.Email.Contains("@hotmail.") && 
                                   !u.Email.Contains("@outlook.") &&
                                   !u.Email.Contains("@yahoo."),
                RegiaoAdministrativa = u.RegiaoAdministrativa != null ? new
                {
                    u.RegiaoAdministrativa.Id,
                    u.RegiaoAdministrativa.Nome,
                    u.RegiaoAdministrativa.Sigla
                } : null
            })
            .ToListAsync();

        return Ok(new { data = instituicoes, total = instituicoes.Count });
    }

    // GET: api/instituicoes/{id} - Obter detalhes de uma instituição
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInstituicao(int id)
    {
        var instituicao = await _context.Usuarios
            .Where(u => u.Id == id && u.Tipo == TiposUsuario.Instituicao)
            .Include(u => u.RegiaoAdministrativa)
            .Include(u => u.AprovadoPor)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Endereco,
                u.Cidade,
                u.Cnpj,
                u.NomeInstituicao,
                u.DescricaoInstituicao,
                u.StatusAprovacao,
                u.DataCriacao,
                u.DataAprovacao,
                u.MotivoRejeicao,
                AprovadoPor = u.AprovadoPor != null ? new { u.AprovadoPor.Id, u.AprovadoPor.Nome } : null,
                RegiaoAdministrativa = u.RegiaoAdministrativa != null ? new
                {
                    u.RegiaoAdministrativa.Id,
                    u.RegiaoAdministrativa.Nome,
                    u.RegiaoAdministrativa.Sigla
                } : null
            })
            .FirstOrDefaultAsync();

        if (instituicao == null)
        {
            return NotFound(new { message = "Instituição não encontrada" });
        }

        return Ok(instituicao);
    }

    // POST: api/instituicoes/{id}/aprovar - Aprovar uma instituição
    [HttpPost("{id}/aprovar")]
    public async Task<IActionResult> AprovarInstituicao(int id)
    {
        var instituicao = await _context.Usuarios.FindAsync(id);

        if (instituicao == null || instituicao.Tipo != TiposUsuario.Instituicao)
        {
            return NotFound(new { message = "Instituição não encontrada" });
        }

        if (instituicao.StatusAprovacao == StatusAprovacoes.Aprovado)
        {
            return BadRequest(new { message = "Esta instituição já está aprovada" });
        }

        instituicao.StatusAprovacao = StatusAprovacoes.Aprovado;
        instituicao.DataAprovacao = DateTime.UtcNow;
        instituicao.AprovadoPorId = User.Id();
        instituicao.MotivoRejeicao = null;
        _context.Registrar(User.Id(), AcoesAuditadas.InstituicaoAprovada, ItensAuditados.Usuario, id,
            $"Aprovou a instituição {instituicao.NomeInstituicao ?? instituicao.Nome}");

        await _context.SaveChangesAsync();

        return Ok(new { message = "Instituição aprovada com sucesso!" });
    }

    // POST: api/instituicoes/{id}/rejeitar - Rejeitar uma instituição
    [HttpPost("{id}/rejeitar")]
    public async Task<IActionResult> RejeitarInstituicao(int id, [FromBody] RejeitarInstituicaoRequest request)
    {
        var instituicao = await _context.Usuarios.FindAsync(id);

        if (instituicao == null || instituicao.Tipo != TiposUsuario.Instituicao)
        {
            return NotFound(new { message = "Instituição não encontrada" });
        }

        if (string.IsNullOrEmpty(request.Motivo))
        {
            return BadRequest(new { message = "É necessário informar o motivo da rejeição" });
        }

        instituicao.StatusAprovacao = StatusAprovacoes.Rejeitado;
        instituicao.MotivoRejeicao = request.Motivo;
        instituicao.DataAprovacao = DateTime.UtcNow;
        instituicao.AprovadoPorId = User.Id();
        _context.Registrar(User.Id(), AcoesAuditadas.InstituicaoRejeitada, ItensAuditados.Usuario, id,
            $"Rejeitou a instituição {instituicao.NomeInstituicao ?? instituicao.Nome}", request.Motivo);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Instituição rejeitada" });
    }

    // GET: api/instituicoes/aprovadas - Listar instituições aprovadas (para delegar campanhas)
    [HttpGet("aprovadas")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInstituicoesAprovadas([FromQuery] int? regiaoId = null)
    {
        var query = _context.Usuarios
            .Where(u => u.Tipo == TiposUsuario.Instituicao && u.StatusAprovacao == StatusAprovacoes.Aprovado)
            .Include(u => u.RegiaoAdministrativa)
            .AsQueryable();

        if (regiaoId.HasValue)
        {
            query = query.Where(u => u.RegiaoAdministrativaId == regiaoId);
        }

        var instituicoes = await query
            .OrderBy(u => u.NomeInstituicao)
            .Select(u => new
            {
                u.Id,
                u.NomeInstituicao,
                u.Email,
                u.Telefone,
                RegiaoAdministrativa = u.RegiaoAdministrativa != null ? new
                {
                    u.RegiaoAdministrativa.Id,
                    u.RegiaoAdministrativa.Nome,
                    u.RegiaoAdministrativa.Sigla
                } : null
            })
            .ToListAsync();

        return Ok(new { data = instituicoes, total = instituicoes.Count });
    }
}

public class RejeitarInstituicaoRequest
{
    public string Motivo { get; set; } = string.Empty;
}
