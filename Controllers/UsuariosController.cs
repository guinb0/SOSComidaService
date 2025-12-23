using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;

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

    // GET: api/usuarios/5 - Buscar usuário por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new {
                u.Id,
                u.Nome,
                u.Email,
                u.Telefone,
                u.Cidade,
                u.Endereco,
                u.Tipo,
                u.DataCriacao
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
            return NotFound(new { message = "Usuário não encontrado" });

        return Ok(usuario);
    }

    // GET: api/usuarios - Listar todos os usuários (apenas para admin/moderador)
    [HttpGet]
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
