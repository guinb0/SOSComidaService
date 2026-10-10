using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.Seguranca;
using SOSComida.Services;

namespace SOSComida.Controllers;

/// <summary>
/// Gestão da equipe de moderação, só pelo admin.
///
/// Antes a tela de moderadores criava, editava, desativava e excluía só na
/// lista em memória do navegador — nada chegava ao banco. "Excluir" aqui
/// rebaixa para usuário comum em vez de apagar a conta, que tem histórico
/// (avisos enviados, campanhas aprovadas).
/// </summary>
[ApiController]
[Route("api/moderadores")]
[Authorize(Policy = Politicas.Admin)]
public class ModeradoresController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ModeradoresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/moderadores - Admins e moderadores, com as regiões de cada um
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var equipe = await _context.Usuarios
            .Where(u => u.Tipo == TiposUsuario.Moderador || u.Tipo == TiposUsuario.Admin)
            .OrderByDescending(u => u.Tipo == TiposUsuario.Admin)
            .ThenBy(u => u.Nome)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Cidade,
                u.Tipo,
                u.Ativo,
                u.DataCriacao,
                Regioes = _context.ModeradorRegioes
                    .Where(mr => mr.ModeradorId == u.Id && mr.Ativo)
                    .Select(mr => new { mr.Regiao.Id, mr.Regiao.Nome, mr.Regiao.Sigla })
                    .ToList()
            })
            .ToListAsync();

        return Ok(new { data = equipe, total = equipe.Count });
    }

    // GET: api/moderadores/historico - Ações da equipe, mais recentes primeiro
    [HttpGet("historico")]
    public async Task<IActionResult> Historico([FromQuery] int limite = 500)
    {
        var acoes = await _context.AcoesModeracao
            .OrderByDescending(a => a.DataAcao)
            .Take(Math.Clamp(limite, 1, 2000))
            .Select(a => new
            {
                a.Id,
                a.ModeradorId,
                ModeradorNome = a.Moderador.Nome,
                a.TipoAcao,
                Descricao = a.Motivo,
                a.Observacoes,
                Data = a.DataAcao,
                a.TipoItem,
                a.ItemId,
                Cidade = a.Moderador.Cidade,
                // Região da campanha moderada; para ações sobre pessoas, nula.
                Regiao = a.TipoItem == ItensAuditados.Campanha
                    ? _context.Campanhas.Where(c => c.Id == a.ItemId).Select(c => c.Regiao!.Nome).FirstOrDefault()
                    : null
            })
            .ToListAsync();

        return Ok(new { data = acoes, total = acoes.Count });
    }

    // POST: api/moderadores - Cria uma conta de moderador
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarModeradorRequest request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Este email já está cadastrado" });

        var moderador = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = SenhaService.Hash(request.Senha),
            Telefone = request.Telefone ?? "",
            Cidade = request.Cidade ?? "",
            Tipo = TiposUsuario.Moderador,
            // Senha definida pelo admin: a pessoa troca no primeiro acesso.
            SenhaTemporaria = true,
            StatusAprovacao = StatusAprovacoes.NaoAplicavel,
            DataCriacao = DateTime.UtcNow
        };

        _context.Usuarios.Add(moderador);
        await _context.SaveChangesAsync();
        _context.Registrar(User.Id(), AcoesAuditadas.ModeradorCriado, ItensAuditados.Usuario, moderador.Id,
            $"Criou o moderador {moderador.Nome}");
        await _context.SaveChangesAsync();

        return Ok(new { message = "Moderador criado com sucesso!", id = moderador.Id });
    }

    // PUT: api/moderadores/5 - Atualiza os dados de contato
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarModeradorRequest request)
    {
        var (moderador, erro) = await BuscarModeradorAsync(id);
        if (moderador == null) return erro!;

        if (request.Email != moderador.Email
            && await _context.Usuarios.AnyAsync(u => u.Email == request.Email && u.Id != id))
            return BadRequest(new { message = "Este email já está cadastrado" });

        moderador.Nome = request.Nome;
        moderador.Email = request.Email;
        moderador.Telefone = request.Telefone ?? "";
        moderador.Cidade = request.Cidade ?? "";
        _context.Registrar(User.Id(), AcoesAuditadas.ModeradorEditado, ItensAuditados.Usuario, id,
            $"Editou os dados de {moderador.Nome}");
        await _context.SaveChangesAsync();

        return Ok(new { message = "Moderador atualizado com sucesso!" });
    }

    // POST: api/moderadores/5/ativar | desativar
    [HttpPost("{id}/ativar")]
    public Task<IActionResult> Ativar(int id) => DefinirAtivoAsync(id, true);

    [HttpPost("{id}/desativar")]
    public Task<IActionResult> Desativar(int id) => DefinirAtivoAsync(id, false);

    // DELETE: api/moderadores/5 - Tira da equipe (vira usuário comum)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(int id)
    {
        var (moderador, erro) = await BuscarModeradorAsync(id);
        if (moderador == null) return erro!;

        moderador.Tipo = TiposUsuario.Usuario;
        _context.Registrar(User.Id(), AcoesAuditadas.ModeradorRemovido, ItensAuditados.Usuario, id,
            $"Tirou {moderador.Nome} da equipe de moderação");
        await _context.ModeradorRegioes
            .Where(mr => mr.ModeradorId == id)
            .ExecuteUpdateAsync(mr => mr.SetProperty(x => x.Ativo, false));
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{moderador.Nome} saiu da equipe de moderação." });
    }

    private async Task<IActionResult> DefinirAtivoAsync(int id, bool ativo)
    {
        var (moderador, erro) = await BuscarModeradorAsync(id);
        if (moderador == null) return erro!;

        moderador.Ativo = ativo;
        _context.Registrar(User.Id(),
            ativo ? AcoesAuditadas.ModeradorAtivado : AcoesAuditadas.ModeradorDesativado,
            ItensAuditados.Usuario, id,
            $"{(ativo ? "Ativou" : "Desativou")} o moderador {moderador.Nome}");
        await _context.SaveChangesAsync();

        return Ok(new { message = ativo ? "Moderador ativado" : "Moderador desativado" });
    }

    /// <summary>Só moderadores: o admin não pode ser editado nem removido por aqui.</summary>
    private async Task<(Usuario? Moderador, IActionResult? Erro)> BuscarModeradorAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return (null, NotFound(new { message = "Moderador não encontrado" }));

        if (usuario.Tipo != TiposUsuario.Moderador)
            return (null, BadRequest(new { message = "Só contas de moderador podem ser alteradas aqui" }));

        return (usuario, null);
    }
}

public class CriarModeradorRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório"), EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
}

public class AtualizarModeradorRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório"), EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
}
