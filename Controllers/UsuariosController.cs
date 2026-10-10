using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Seguranca;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/usuarios/5 - Perfil de um usuário. Contato (e-mail, telefone,
    // endereço) só para o próprio usuário e para a moderação; os demais veem
    // o perfil público.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound(new { message = "Usuário não encontrado" });

        var veContato = usuario.Id == User.Id() || User.EhModerador();

        return Ok(new
        {
            usuario.Id,
            usuario.Nome,
            Email = veContato ? usuario.Email : null,
            Telefone = veContato ? usuario.Telefone : null,
            usuario.Cidade,
            Endereco = veContato ? usuario.Endereco : null,
            usuario.Tipo,
            usuario.FotoUrl,
            usuario.DataCriacao
        });
    }

    // GET: api/usuarios - Listar todos os usuários (apenas para admin/moderador)
    [HttpGet]
    [Authorize(Policy = Politicas.Moderacao)]
    public async Task<IActionResult> GetUsuarios([FromQuery] string? tipo = null)
    {
        var query = _context.Usuarios.AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
        {
            query = query.Where(u => u.Tipo == tipo);
        }

        var usuarios = await query
            .Select(u => new {
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Cidade,
                u.Tipo,
                u.DataCriacao
            })
            .OrderBy(u => u.Nome)
            .ToListAsync();

        return Ok(new { data = usuarios, total = usuarios.Count });
    }
}
