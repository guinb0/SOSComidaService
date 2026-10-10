using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Models;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;
using SOSComida.Seguranca;
using SOSComida.Services;

namespace SOSComida.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private static readonly TimeSpan ValidadeSenhaRecuperacao = TimeSpan.FromHours(1);

    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IRecaptchaService _recaptcha;
    private readonly TokenService _tokens;

    public AuthController(
        ApplicationDbContext context,
        IEmailService emailService,
        IRecaptchaService recaptcha,
        TokenService tokens)
    {
        _context = context;
        _emailService = emailService;
        _recaptcha = recaptcha;
        _tokens = tokens;
    }

    private static string GerarSenhaTemporaria(int tamanho = 10)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        return RandomNumberGenerator.GetString(chars, tamanho);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(LimitesDeTaxa.Autenticacao)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (!await _recaptcha.ValidarAsync(request.RecaptchaToken, ip))
            return BadRequest(new { message = "Confirme o CAPTCHA para entrar" });

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
            return Unauthorized(new { message = "Usuário ou senha inválidos" });

        if (!usuario.Ativo)
            return Unauthorized(new { message = "Esta conta está desativada" });

        var usouRecuperacao = false;
        if (!SenhaService.Confere(usuario.Senha, request.Senha))
        {
            usouRecuperacao = SenhaRecuperacaoValida(usuario, request.Senha);
            if (!usouRecuperacao)
                return Unauthorized(new { message = "Usuário ou senha inválidos" });
        }

        return Ok(new LoginResponse
        {
            Token = _tokens.Gerar(usuario),
            Usuario = UsuarioDto.De(usuario),
            SenhaTemporaria = usuario.SenhaTemporaria || usouRecuperacao
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(LimitesDeTaxa.Autenticacao)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existeEmail = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (existeEmail)
        {
            return BadRequest(new { message = "Este email já está cadastrado" });
        }

        // Validações específicas para instituições
        var isInstituicao = request.Tipo == TiposUsuario.Instituicao;
        if (isInstituicao)
        {
            if (!request.RegiaoAdministrativaId.HasValue)
            {
                return BadRequest(new { message = "Instituições devem selecionar uma região administrativa" });
            }

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

        // O tipo nunca vem do cliente além de "usuario"/"instituicao": sem isso
        // daria para se cadastrar direto como moderador ou admin.
        var novoUsuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = SenhaService.Hash(request.Senha),
            Telefone = request.Telefone ?? "",
            Endereco = request.Endereco ?? "",
            Cidade = request.Cidade ?? "",
            Cpf = isInstituicao ? null : request.Cpf,
            Tipo = isInstituicao ? TiposUsuario.Instituicao : TiposUsuario.Usuario,
            SenhaTemporaria = false,
            DataCriacao = DateTime.UtcNow,
            Cnpj = isInstituicao ? request.Cnpj : null,
            RegiaoAdministrativaId = isInstituicao ? request.RegiaoAdministrativaId : null,
            NomeInstituicao = isInstituicao ? request.NomeInstituicao : null,
            DescricaoInstituicao = isInstituicao ? request.DescricaoInstituicao : null,
            StatusAprovacao = isInstituicao ? StatusAprovacoes.Pendente : StatusAprovacoes.NaoAplicavel
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

    [HttpPost("alterar-senha")]
    [EnableRateLimiting(LimitesDeTaxa.Autenticacao)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(User.Id());
        if (usuario == null)
            return Unauthorized();

        var senhaAtualConfere = SenhaService.Confere(usuario.Senha, request.SenhaAtual)
                                || SenhaRecuperacaoValida(usuario, request.SenhaAtual);
        if (!senhaAtualConfere)
        {
            return BadRequest(new { message = "Senha atual incorreta" });
        }

        usuario.Senha = SenhaService.Hash(request.NovaSenha);
        usuario.SenhaTemporaria = false;
        usuario.SenhaRecuperacaoHash = null;
        usuario.SenhaRecuperacaoExpiraEm = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Senha alterada com sucesso!" });
    }

    /// <summary>
    /// Gera uma senha temporária com validade curta, sem mexer na senha atual.
    /// Antes a senha era trocada na hora: qualquer pessoa que soubesse um
    /// e-mail cadastrado conseguia derrubar a senha do dono.
    /// </summary>
    [HttpPost("reenviar-senha")]
    [AllowAnonymous]
    [EnableRateLimiting(LimitesDeTaxa.Autenticacao)]
    public async Task<IActionResult> ReenviarSenha([FromBody] ReenviarSenhaRequest request)
    {
        const string resposta = "Se o email estiver cadastrado, uma senha temporária será enviada.";

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (usuario == null)
        {
            return Ok(new { message = resposta });
        }

        var senhaTemporaria = GerarSenhaTemporaria();
        usuario.SenhaRecuperacaoHash = SenhaService.Hash(senhaTemporaria);
        usuario.SenhaRecuperacaoExpiraEm = DateTime.UtcNow.Add(ValidadeSenhaRecuperacao);
        await _context.SaveChangesAsync();

        await _emailService.EnviarSenhaRecuperacao(usuario.Email, usuario.Nome, senhaTemporaria, ValidadeSenhaRecuperacao);

        return Ok(new { message = resposta });
    }

    [HttpPut("perfil/{id}")]
    public async Task<IActionResult> AtualizarPerfil(int id, [FromBody] AtualizarPerfilRequest request)
    {
        if (id != User.Id())
            return Forbid();

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
        return Ok(UsuarioDto.De(usuario));
    }

    [HttpPost("perfil/{id}/foto")]
    [RequestSizeLimit(TamanhoMaximoFoto + 64 * 1024)]
    public async Task<IActionResult> UploadFoto(int id, IFormFile foto)
    {
        if (id != User.Id())
            return Forbid();

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        if (foto == null || foto.Length == 0)
        {
            return BadRequest(new { message = "Nenhuma imagem enviada" });
        }

        if (foto.Length > TamanhoMaximoFoto)
        {
            return BadRequest(new { message = "A imagem deve ter no máximo 5MB" });
        }

        var extensao = await ImagemUpload.DetectarExtensaoAsync(foto);
        if (extensao == null)
        {
            return BadRequest(new { message = "Tipo de arquivo não permitido. Use JPG, PNG, GIF ou WebP." });
        }

        var (url, _) = await ImagemUpload.SalvarAsync(foto, extensao, "fotos", id.ToString());

        ImagemUpload.Apagar(usuario.FotoUrl);
        usuario.FotoUrl = url;
        await _context.SaveChangesAsync();

        return Ok(new { fotoUrl = usuario.FotoUrl, message = "Foto atualizada com sucesso!" });
    }

    [HttpDelete("perfil/{id}/foto")]
    public async Task<IActionResult> RemoverFoto(int id)
    {
        if (id != User.Id())
            return Forbid();

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        ImagemUpload.Apagar(usuario.FotoUrl);
        usuario.FotoUrl = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Foto removida com sucesso!" });
    }

    private const long TamanhoMaximoFoto = 5 * 1024 * 1024;

    private static bool SenhaRecuperacaoValida(Usuario usuario, string senha) =>
        usuario.SenhaRecuperacaoExpiraEm > DateTime.UtcNow
        && SenhaService.Confere(usuario.SenhaRecuperacaoHash, senha);
}

public class AtualizarPerfilRequest
{
    public string? Nome { get; set; }
    public string? Telefone { get; set; }
    public string? Cidade { get; set; }
    public string? Endereco { get; set; }
}
