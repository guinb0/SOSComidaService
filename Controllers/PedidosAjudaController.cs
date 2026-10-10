using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.Seguranca;

namespace SOSComida.Controllers;

[ApiController]
[Route("api/pedidos-ajuda")]
public class PedidosAjudaController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PedidosAjudaController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/pedidos-ajuda - Listar todos os pedidos
    [HttpGet]
    public async Task<IActionResult> GetPedidos(
        [FromQuery] string? status = null,
        [FromQuery] string? urgencia = null,
        [FromQuery] int? regiaoId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.PedidosAjuda
            .Include(p => p.Usuario)
            .Include(p => p.Regiao)
            .Include(p => p.Campanha)
            .AsQueryable();

        // Filtrar apenas pedidos ativos (não cancelados)
        if (string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status != StatusPedidos.Cancelado);
        }
        else
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrEmpty(urgencia))
        {
            query = query.Where(p => p.Urgencia == urgencia);
        }

        if (regiaoId.HasValue)
        {
            query = query.Where(p => p.RegiaoId == regiaoId);
        }

        var total = await query.CountAsync();

        var pedidos = await query
            .OrderByDescending(p => p.Urgencia == UrgenciasPedido.Critica ? 4 : p.Urgencia == UrgenciasPedido.Alta ? 3 : p.Urgencia == UrgenciasPedido.Media ? 2 : 1)
            .ThenByDescending(p => p.DataCriacao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Titulo,
                p.Descricao,
                p.Localizacao,
                p.Urgencia,
                p.Status,
                p.QuantidadePessoas,
                p.TipoAjuda,
                p.Telefone,
                p.DataCriacao,
                p.DataAtendimento,
                Usuario = new { p.Usuario.Id, p.Usuario.Nome },
                Regiao = p.Regiao != null ? new { p.Regiao.Id, p.Regiao.Nome, p.Regiao.Sigla } : null,
                Campanha = p.Campanha != null ? new { p.Campanha.Id, p.Campanha.Titulo } : null
            })
            .ToListAsync();

        return Ok(new { data = pedidos, total, page, pageSize });
    }

    // GET: api/pedidos-ajuda/{id} - Obter detalhes de um pedido
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPedido(int id)
    {
        var pedido = await _context.PedidosAjuda
            .Include(p => p.Usuario)
            .Include(p => p.Regiao)
            .Include(p => p.Campanha)
            .Include(p => p.AtendidoPor)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Titulo,
                p.Descricao,
                p.Localizacao,
                p.Urgencia,
                p.Status,
                p.QuantidadePessoas,
                p.TipoAjuda,
                p.Telefone,
                p.Observacoes,
                p.DataCriacao,
                p.DataAtualizacao,
                p.DataAtendimento,
                Usuario = new { p.Usuario.Id, p.Usuario.Nome, p.Usuario.Email, p.Usuario.Telefone },
                Regiao = p.Regiao != null ? new { p.Regiao.Id, p.Regiao.Nome, p.Regiao.Sigla } : null,
                Campanha = p.Campanha != null ? new { p.Campanha.Id, p.Campanha.Titulo } : null,
                AtendidoPor = p.AtendidoPor != null ? new { p.AtendidoPor.Id, p.AtendidoPor.Nome } : null
            })
            .FirstOrDefaultAsync();

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido de ajuda não encontrado" });
        }

        return Ok(pedido);
    }

    // GET: api/pedidos-ajuda/meus - Pedidos do usuário logado
    [HttpGet("meus/{usuarioId}")]
    public async Task<IActionResult> GetMeusPedidos(int usuarioId)
    {
        if (usuarioId != User.Id())
            return Forbid();

        var pedidos = await _context.PedidosAjuda
            .Where(p => p.UsuarioId == usuarioId)
            .Include(p => p.Regiao)
            .OrderByDescending(p => p.DataCriacao)
            .Select(p => new
            {
                p.Id,
                p.Titulo,
                p.Descricao,
                p.Localizacao,
                p.Urgencia,
                p.Status,
                p.QuantidadePessoas,
                p.TipoAjuda,
                p.DataCriacao,
                p.DataAtendimento,
                Regiao = p.Regiao != null ? new { p.Regiao.Id, p.Regiao.Nome, p.Regiao.Sigla } : null
            })
            .ToListAsync();

        return Ok(new { data = pedidos, total = pedidos.Count });
    }

    // POST: api/pedidos-ajuda - Criar novo pedido
    [HttpPost]
    public async Task<IActionResult> CreatePedido([FromBody] CreatePedidoAjudaRequest request)
    {
        // Validar região se informada
        if (request.RegiaoId.HasValue)
        {
            var regiaoExiste = await _context.RegioesAdministrativas
                .AnyAsync(r => r.Id == request.RegiaoId && r.Ativa);
            if (!regiaoExiste)
            {
                return BadRequest(new { message = "Região administrativa inválida" });
            }
        }

        var pedido = new PedidoAjuda
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            Localizacao = request.Localizacao,
            Urgencia = request.Urgencia ?? UrgenciasPedido.Media,
            Status = StatusPedidos.Pendente,
            QuantidadePessoas = request.QuantidadePessoas > 0 ? request.QuantidadePessoas : 1,
            TipoAjuda = request.TipoAjuda,
            Telefone = request.Telefone,
            Observacoes = request.Observacoes,
            UsuarioId = User.Id(),
            RegiaoId = request.RegiaoId,
            DataCriacao = DateTime.UtcNow
        };

        _context.PedidosAjuda.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, new
        {
            message = "Pedido de ajuda criado com sucesso!",
            pedido = new { pedido.Id, pedido.Titulo, pedido.Status, pedido.Urgencia }
        });
    }

    // PUT: api/pedidos-ajuda/{id} - Atualizar pedido
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePedido(int id, [FromBody] UpdatePedidoAjudaRequest request)
    {
        var pedido = await _context.PedidosAjuda.FindAsync(id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido de ajuda não encontrado" });
        }

        // Verificar se o usuário é o dono do pedido
        if (pedido.UsuarioId != User.Id())
        {
            return StatusCode(403, new { message = "Você não tem permissão para editar este pedido" });
        }

        // Não permitir edição de pedidos já atendidos
        if (pedido.Status == StatusPedidos.Atendido)
        {
            return BadRequest(new { message = "Não é possível editar um pedido já atendido" });
        }

        pedido.Titulo = request.Titulo ?? pedido.Titulo;
        pedido.Descricao = request.Descricao ?? pedido.Descricao;
        pedido.Localizacao = request.Localizacao ?? pedido.Localizacao;
        pedido.Urgencia = request.Urgencia ?? pedido.Urgencia;
        pedido.QuantidadePessoas = request.QuantidadePessoas > 0 ? request.QuantidadePessoas : pedido.QuantidadePessoas;
        pedido.TipoAjuda = request.TipoAjuda ?? pedido.TipoAjuda;
        pedido.Telefone = request.Telefone ?? pedido.Telefone;
        pedido.Observacoes = request.Observacoes ?? pedido.Observacoes;
        pedido.RegiaoId = request.RegiaoId ?? pedido.RegiaoId;
        pedido.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Pedido atualizado com sucesso!" });
    }

    // POST: api/pedidos-ajuda/{id}/atender - Marcar pedido como em atendimento
    [HttpPost("{id}/atender")]
    public async Task<IActionResult> AtenderPedido(int id, [FromBody] AtenderPedidoRequest request)
    {
        var pedido = await _context.PedidosAjuda.FindAsync(id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido de ajuda não encontrado" });
        }

        if (pedido.Status != StatusPedidos.Pendente)
        {
            return BadRequest(new { message = "Este pedido já está sendo atendido ou foi concluído" });
        }

        pedido.Status = StatusPedidos.EmAndamento;
        pedido.AtendidoPorId = User.Id();
        pedido.CampanhaId = request.CampanhaId;
        pedido.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Você está atendendo este pedido!" });
    }

    // POST: api/pedidos-ajuda/{id}/concluir - Marcar pedido como atendido
    [HttpPost("{id}/concluir")]
    public async Task<IActionResult> ConcluirPedido(int id, [FromBody] ConcluirPedidoRequest request)
    {
        var pedido = await _context.PedidosAjuda.FindAsync(id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido de ajuda não encontrado" });
        }

        // Verificar se quem está concluindo é o solicitante ou quem atendeu
        var usuarioId = User.Id();
        if (pedido.UsuarioId != usuarioId && pedido.AtendidoPorId != usuarioId)
        {
            return StatusCode(403, new { message = "Você não tem permissão para concluir este pedido" });
        }

        pedido.Status = StatusPedidos.Atendido;
        pedido.DataAtendimento = DateTime.UtcNow;
        pedido.DataAtualizacao = DateTime.UtcNow;
        pedido.Observacoes = string.IsNullOrEmpty(request.Observacoes) 
            ? pedido.Observacoes 
            : $"{pedido.Observacoes}\n[Conclusão]: {request.Observacoes}";

        await _context.SaveChangesAsync();

        return Ok(new { message = "Pedido marcado como atendido!" });
    }

    // POST: api/pedidos-ajuda/{id}/cancelar - Cancelar pedido
    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> CancelarPedido(int id, [FromBody] CancelarPedidoRequest request)
    {
        var pedido = await _context.PedidosAjuda.FindAsync(id);

        if (pedido == null)
        {
            return NotFound(new { message = "Pedido de ajuda não encontrado" });
        }

        // Apenas o solicitante pode cancelar
        if (pedido.UsuarioId != User.Id())
        {
            return StatusCode(403, new { message = "Você não tem permissão para cancelar este pedido" });
        }

        if (pedido.Status == StatusPedidos.Atendido)
        {
            return BadRequest(new { message = "Não é possível cancelar um pedido já atendido" });
        }

        pedido.Status = StatusPedidos.Cancelado;
        pedido.DataAtualizacao = DateTime.UtcNow;
        pedido.Observacoes = string.IsNullOrEmpty(request.Motivo) 
            ? pedido.Observacoes 
            : $"{pedido.Observacoes}\n[Cancelado]: {request.Motivo}";

        await _context.SaveChangesAsync();

        return Ok(new { message = "Pedido cancelado" });
    }

    // GET: api/pedidos-ajuda/estatisticas - Estatísticas gerais
    [HttpGet("estatisticas")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEstatisticas()
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var estatisticas = new
        {
            TotalPedidos = await _context.PedidosAjuda.CountAsync(),
            Pendentes = await _context.PedidosAjuda.CountAsync(p => p.Status == StatusPedidos.Pendente),
            EmAndamento = await _context.PedidosAjuda.CountAsync(p => p.Status == StatusPedidos.EmAndamento),
            Atendidos = await _context.PedidosAjuda.CountAsync(p => p.Status == StatusPedidos.Atendido),
            AtendidosEsteMes = await _context.PedidosAjuda.CountAsync(p => p.Status == StatusPedidos.Atendido && p.DataAtendimento >= inicioMes),
            Criticos = await _context.PedidosAjuda.CountAsync(p => p.Urgencia == UrgenciasPedido.Critica && p.Status == StatusPedidos.Pendente),
            PessoasAjudadas = await _context.PedidosAjuda.Where(p => p.Status == StatusPedidos.Atendido).SumAsync(p => p.QuantidadePessoas)
        };

        return Ok(estatisticas);
    }
}

// Request DTOs
public class CreatePedidoAjudaRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Informe o título")]
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string? Urgencia { get; set; }
    public int QuantidadePessoas { get; set; } = 1;
    public string? TipoAjuda { get; set; }
    public string? Telefone { get; set; }
    public string? Observacoes { get; set; }
    public int? RegiaoId { get; set; }
}

public class UpdatePedidoAjudaRequest
{
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? Localizacao { get; set; }
    public string? Urgencia { get; set; }
    public int QuantidadePessoas { get; set; }
    public string? TipoAjuda { get; set; }
    public string? Telefone { get; set; }
    public string? Observacoes { get; set; }
    public int? RegiaoId { get; set; }
}

public class AtenderPedidoRequest
{
    public int? CampanhaId { get; set; }
}

public class ConcluirPedidoRequest
{
    public string? Observacoes { get; set; }
}

public class CancelarPedidoRequest
{
    public string? Motivo { get; set; }
}
