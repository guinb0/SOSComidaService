using Microsoft.AspNetCore.Mvc;
using SOSComida.Models;

namespace SOSComida.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Autenticação simples para teste - usuário: guinb, senha: 123
        if (request.Email == "guinb" && request.Senha == "123")
        {
            var response = new LoginResponse
            {
                Token = "mock-jwt-token-12345",
                Usuario = new Usuario
                {
                    Id = 1,
                    Nome = "Guinb",
                    Email = "guinb@example.com",
                    Telefone = "123456789",
                    Endereco = "Rua Exemplo, 123",
                    Cidade = "São Paulo",
                    Cpf = "12345678900",
                    Tipo = "usuario"
                }
            };
            return Ok(response);
        }

        return Unauthorized(new { message = "Usuário ou senha inválidos" });
    }
}
