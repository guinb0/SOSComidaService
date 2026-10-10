using System.Net;
using System.Net.Http.Json;

namespace SOSComida.Tests;

/// <summary>Fábrica própria: aqui o limite padrão (10/min) precisa valer.</summary>
public class ApiComLimitePadrao : ApiFactory
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("LimitesDeTaxa:AutenticacaoPorMinuto", "10");
    }
}

public class LimiteDeTaxaTests : IClassFixture<ApiComLimitePadrao>
{
    private readonly ApiComLimitePadrao _api;

    public LimiteDeTaxaTests(ApiComLimitePadrao api) => _api = api;

    [Fact]
    public async Task Bloqueia_tentativas_de_senha_em_sequencia()
    {
        var cliente = _api.CreateClient();
        HttpStatusCode ultimo = default;

        for (var i = 0; i < 11; i++)
        {
            var resposta = await cliente.PostAsJsonAsync("/auth/login",
                new { email = "doador@teste.com", senha = $"chute{i}", recaptchaToken = "ok" });
            ultimo = resposta.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, ultimo);
    }
}
