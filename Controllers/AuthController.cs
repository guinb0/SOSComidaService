using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Services;

namespace SOSComida.Controllers;

[ApiController]
[Authorize]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISenhaService _senhas;
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenService _refresh;

    public AuthController(
        ApplicationDbContext context,
        IEmailService emailService,
        ISenhaService senhas,
        ITokenService tokens,
        IRefreshTokenService refresh)
    {
        _context = context;
        _emailService = emailService;
        _senhas = senhas;
        _tokens = tokens;
        _refresh = refresh;
    }

    private static string GerarSenhaTemporaria(int tamanho = 8)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        // RandomNumberGenerator, não Random(): a senha temporária vai por e-mail e
        // dá acesso à conta — não pode ser adivinhável a partir do relógio.
        return System.Security.Cryptography.RandomNumberGenerator.GetString(chars, tamanho);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

        // A senha era comparada em texto puro no próprio WHERE do banco. Agora a
        // conferência é sobre o hash, e fora da consulta.
        if (usuario is null || !_senhas.Confere(request.Senha, usuario.Senha))
        {
            return Unauthorized(new { message = "Usuário ou senha inválidos" });
        }

        // Conta antiga, com a senha ainda em texto puro (ou hash com custo menor):
        // aproveita que a senha está em mãos e regrava no formato atual.
        if (_senhas.PrecisaRehash(usuario.Senha))
        {
            usuario.Senha = _senhas.Hash(request.Senha);
            await _context.SaveChangesAsync();
        }

        return Ok(new LoginResponse
        {
            Token = _tokens.Gerar(usuario),
            RefreshToken = await _refresh.EmitirAsync(usuario.Id),
            Usuario = UsuarioDto.De(usuario), // sem a senha
            SenhaTemporaria = usuario.SenhaTemporaria,
        });
    }

    /// <summary>
    /// Troca um refresh token válido por um novo par (access + refresh), sem pedir
    /// a senha. O refresh é rotacionado a cada chamada — o antigo deixa de valer.
    /// É este o endpoint que o interceptor do axios no frontend já esperava.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token ausente" });

        var resultado = await _refresh.RotacionarAsync(request.RefreshToken);
        if (resultado is null)
            return Unauthorized(new { message = "Sessão expirada. Faça login novamente." });

        var (novoRefresh, usuario) = resultado.Value;
        return Ok(new LoginResponse
        {
            Token = _tokens.Gerar(usuario),
            RefreshToken = novoRefresh,
            Usuario = UsuarioDto.De(usuario),
            SenhaTemporaria = usuario.SenhaTemporaria,
        });
    }

    /// <summary>Revoga o refresh token (logout). Anônimo: o próprio token é a prova.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            await _refresh.RevogarAsync(request.RefreshToken);

        return Ok(new { message = "Sessão encerrada." });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Verificar se email já existe
        var existeEmail = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (existeEmail)
        {
            return BadRequest(new { message = "Este email já está cadastrado" });
        }

        // Validar senha
        if (string.IsNullOrEmpty(request.Senha) || request.Senha.Length < 6)
        {
            return BadRequest(new { message = "A senha deve ter pelo menos 6 caracteres" });
        }

        // Validações específicas para instituições
        var isInstituicao = request.Tipo == "instituicao";
        if (isInstituicao)
        {
            if (!request.RegiaoAdministrativaId.HasValue)
            {
                return BadRequest(new { message = "Instituições devem selecionar uma região administrativa" });
            }
            
            // Verificar se a região existe
            var regiaoExiste = await _context.RegioesAdministrativas
                .AnyAsync(r => r.Id == request.RegiaoAdministrativaId && r.Ativa);
            if (!regiaoExiste)
            {
                return BadRequest(new { message = "Região administrativa inválida" });
            }

            if (string.IsNullOrEmpty(request.NomeInstituicao))
            {
                return BadRequest(new { message = "O nome da instituição é obrigatório" });
            }
        }

        // Criar novo usuário. A senha nunca é gravada como veio.
        var novoUsuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = _senhas.Hash(request.Senha),
            Telefone = request.Telefone ?? "",
            Endereco = request.Endereco ?? "",
            Cidade = request.Cidade ?? "",
            Cpf = isInstituicao ? null : request.Cpf, // CPF apenas para pessoas físicas
            Tipo = isInstituicao ? "instituicao" : "usuario",
            SenhaTemporaria = false,
            DataCriacao = DateTime.UtcNow,
            // Campos de instituição
            Cnpj = isInstituicao ? request.Cnpj : null,
            RegiaoAdministrativaId = isInstituicao ? request.RegiaoAdministrativaId : null,
            NomeInstituicao = isInstituicao ? request.NomeInstituicao : null,
            DescricaoInstituicao = isInstituicao ? request.DescricaoInstituicao : null,
            StatusAprovacao = isInstituicao ? "pendente" : "nao_aplicavel"
        };

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();

        var mensagem = isInstituicao 
            ? "Cadastro realizado! Sua instituição será analisada pelos moderadores. Recomendamos usar um email corporativo para facilitar a validação."
            : "Usuário criado com sucesso! Você já pode fazer login.";

        return Ok(new { 
            message = mensagem,
            email = request.Email,
            usuario = new
            {
                id = novoUsuario.Id,
                nome = novoUsuario.Nome,
                email = novoUsuario.Email,
                tipo = novoUsuario.Tipo,
                statusAprovacao = novoUsuario.StatusAprovacao
            },
            aguardandoAprovacao = isInstituicao
        });
    }

    /// <summary>
    /// Anônimo de propósito: é o fluxo de quem entrou com senha temporária e ainda
    /// não tem token. A senha atual é a prova de identidade.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost("alterar-senha")]
    [AllowAnonymous]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == request.UsuarioId);

        if (usuario is null || !_senhas.Confere(request.SenhaAtual, usuario.Senha))
        {
            return BadRequest(new { message = "Senha atual incorreta" });
        }

        if (string.IsNullOrWhiteSpace(request.NovaSenha) || request.NovaSenha.Length < 6)
        {
            return BadRequest(new { message = "A nova senha deve ter pelo menos 6 caracteres" });
        }

        usuario.Senha = _senhas.Hash(request.NovaSenha);
        usuario.SenhaTemporaria = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Senha alterada com sucesso!" });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("reenviar-senha")]
    [AllowAnonymous]
    public async Task<IActionResult> ReenviarSenha([FromBody] ReenviarSenhaRequest request)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null)
        {
            // Não revelar se o email existe ou não
            return Ok(new { message = "Se o email estiver cadastrado, uma nova senha será enviada." });
        }

        // Gerar nova senha temporária. Guarda o hash; o texto puro só existe aqui,
        // o tempo de ir para o e-mail.
        var novaSenha = GerarSenhaTemporaria();
        usuario.Senha = _senhas.Hash(novaSenha);
        usuario.SenhaTemporaria = true;
        await _context.SaveChangesAsync();

        // Enviar email de recuperação de senha
        await _emailService.EnviarEmailRecuperacaoSenha(usuario.Email, usuario.Nome, novaSenha);

        return Ok(new { message = "Se o email estiver cadastrado, uma nova senha será enviada." });
    }

    [HttpPut("perfil/{id}")]
    public async Task<IActionResult> AtualizarPerfil(int id, [FromBody] AtualizarPerfilRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        if (!string.IsNullOrEmpty(request.Nome)) usuario.Nome = request.Nome;
        if (!string.IsNullOrEmpty(request.Telefone)) usuario.Telefone = request.Telefone;
        if (!string.IsNullOrEmpty(request.Cidade)) usuario.Cidade = request.Cidade;
        if (!string.IsNullOrEmpty(request.Endereco)) usuario.Endereco = request.Endereco;

        await _context.SaveChangesAsync();
        return Ok(usuario);
    }

    [HttpPost("perfil/{id}/foto")]
    public async Task<IActionResult> UploadFoto(int id, IFormFile foto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        if (foto == null || foto.Length == 0)
        {
            return BadRequest(new { message = "Nenhuma imagem enviada" });
        }

        // Validar tipo de arquivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(foto.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Tipo de arquivo não permitido. Use JPG, PNG, GIF ou WebP." });
        }

        // Validar tamanho (máx 5MB)
        if (foto.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "A imagem deve ter no máximo 5MB" });
        }

        // Criar pasta de uploads se não existir
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "fotos");
        Directory.CreateDirectory(uploadsFolder);

        // Gerar nome único para o arquivo
        var extension = Path.GetExtension(foto.FileName);
        var fileName = $"{id}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Deletar foto anterior se existir
        if (!string.IsNullOrEmpty(usuario.FotoUrl))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", usuario.FotoUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        // Salvar nova foto
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await foto.CopyToAsync(stream);
        }

        // Atualizar URL no banco
        usuario.FotoUrl = $"/uploads/fotos/{fileName}";
        await _context.SaveChangesAsync();

        return Ok(new { fotoUrl = usuario.FotoUrl, message = "Foto atualizada com sucesso!" });
    }

    [HttpDelete("perfil/{id}/foto")]
    public async Task<IActionResult> RemoverFoto(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        if (!string.IsNullOrEmpty(usuario.FotoUrl))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", usuario.FotoUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        usuario.FotoUrl = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Foto removida com sucesso!" });
    }
}

public class AtualizarPerfilRequest
{
    public string? Nome { get; set; }
    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
    public string? Endereco { get; set; }
}
