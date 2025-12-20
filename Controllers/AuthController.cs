using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Services;

namespace SOSComida.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public AuthController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    private string GerarSenhaTemporaria(int tamanho = 8)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, tamanho)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Buscar usuário no banco
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Senha == request.Senha);

        if (usuario != null)
        {
            var response = new LoginResponse
            {
                Token = $"mock-jwt-token-{usuario.Id}-{Guid.NewGuid().ToString()[..8]}",
                Usuario = usuario,
                SenhaTemporaria = usuario.SenhaTemporaria
            };
            return Ok(response);
        }

        return Unauthorized(new { message = "Usuário ou senha inválidos" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Verificar se email já existe
        var existeEmail = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (existeEmail)
        {
            return BadRequest(new { message = "Este email já está cadastrado" });
        }

        // Gerar senha temporária
        var senhaTemporaria = GerarSenhaTemporaria();

        // Criar novo usuário
        var novoUsuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = senhaTemporaria,
            Telefone = request.Telefone ?? "",
            Endereco = request.Endereco ?? "",
            Cidade = request.Cidade ?? "",
            Cpf = request.Cpf,
            Tipo = "usuario",
            SenhaTemporaria = true,
            DataCriacao = DateTime.UtcNow
        };

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();

        // Enviar email com senha temporária
        await _emailService.EnviarEmailBoasVindas(request.Email, request.Nome, senhaTemporaria);

        return Ok(new { 
            message = "Usuário criado com sucesso! Uma senha temporária foi enviada para o seu email.",
            email = request.Email,
            usuario = new
            {
                id = novoUsuario.Id,
                nome = novoUsuario.Nome,
                email = novoUsuario.Email,
                tipo = novoUsuario.Tipo
            }
        });
    }

    [HttpPost("alterar-senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.Senha == request.SenhaAtual);

        if (usuario == null)
        {
            return BadRequest(new { message = "Senha atual incorreta" });
        }

        usuario.Senha = request.NovaSenha;
        usuario.SenhaTemporaria = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Senha alterada com sucesso!" });
    }

    [HttpPost("reenviar-senha")]
    public async Task<IActionResult> ReenviarSenha([FromBody] ReenviarSenhaRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null)
        {
            // Não revelar se o email existe ou não
            return Ok(new { message = "Se o email estiver cadastrado, uma nova senha será enviada." });
        }

        // Gerar nova senha temporária
        var novaSenha = GerarSenhaTemporaria();
        usuario.Senha = novaSenha;
        usuario.SenhaTemporaria = true;
        await _context.SaveChangesAsync();

        // Enviar email
        await _emailService.EnviarEmailBoasVindas(usuario.Email, usuario.Nome, novaSenha);

        return Ok(new { message = "Se o email estiver cadastrado, uma nova senha será enviada." });
    }
}
