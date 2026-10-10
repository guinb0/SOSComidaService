using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SOSComida.Services;

namespace SOSComida.Tests;

/// <summary>
/// Sobe a API de verdade com um SQLite temporário populado pelo SeedInicial
/// (senhas: admin123 para o admin, teste123 para os demais).
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _banco = Path.Combine(Path.GetTempPath(), $"soscomida-teste-{Guid.NewGuid()}.db");
    private readonly Dictionary<string, (string Token, int Id)> _sessoes = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_banco}");
        builder.UseSetting("LimitesDeTaxa:AutenticacaoPorMinuto", "1000");
        builder.ConfigureTestServices(services =>
            services.AddSingleton<IRecaptchaService, RecaptchaSempreValido>());
    }

    /// <summary>Um login por usuário, reaproveitando o token entre os testes.</summary>
    public async Task<(HttpClient Cliente, int Id)> ComoAsync(string email, string senha = "teste123")
    {
        if (!_sessoes.TryGetValue(email, out var sessao))
        {
            var resposta = await CreateClient().PostAsJsonAsync("/auth/login",
                new { email, senha, recaptchaToken = "ok" });
            resposta.EnsureSuccessStatusCode();
            var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
            sessao = (json.GetProperty("token").GetString()!, json.GetProperty("usuario").GetProperty("id").GetInt32());
            _sessoes[email] = sessao;
        }

        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessao.Token);
        return (cliente, sessao.Id);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { File.Delete(_banco); } catch (IOException) { }
    }

    private sealed class RecaptchaSempreValido : IRecaptchaService
    {
        public Task<bool> ValidarAsync(string? token, string? ip) => Task.FromResult(!string.IsNullOrEmpty(token));
    }
}

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _api;

    public ApiTests(ApiFactory api) => _api = api;

    [Fact]
    public async Task Leitura_publica_de_campanhas_nao_exige_login()
    {
        var resposta = await _api.CreateClient().GetAsync("/api/campanhas");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Endpoint_protegido_sem_token_devolve_401()
    {
        var resposta = await _api.CreateClient().GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_forjado_no_formato_antigo_nao_vale()
    {
        var cliente = _api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "mock-jwt-token-1-abcdef12");
        var resposta = await cliente.GetAsync("/api/notificacoes/usuario/1");
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Usuario_comum_nao_lista_usuarios()
    {
        var (cliente, _) = await _api.ComoAsync("doador@teste.com");
        var resposta = await cliente.GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Moderador_acessa_a_fila_de_moderacao()
    {
        var (cliente, _) = await _api.ComoAsync("moderador@teste.com");
        var resposta = await cliente.GetAsync("/api/campanhas/moderacao");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Nao_edita_o_perfil_de_outra_pessoa()
    {
        var (_, adminId) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        var (cliente, _) = await _api.ComoAsync("doador@teste.com");

        var resposta = await cliente.PutAsJsonAsync($"/auth/perfil/{adminId}", new { nome = "invadido" });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Nao_le_notificacoes_de_outra_pessoa()
    {
        var (_, adminId) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        var (cliente, _) = await _api.ComoAsync("doador@teste.com");

        var resposta = await cliente.GetAsync($"/api/notificacoes/usuario/{adminId}");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_nao_devolve_a_senha()
    {
        var resposta = await _api.CreateClient().PostAsJsonAsync("/auth/login",
            new { email = "juliana@teste.com", senha = "teste123", recaptchaToken = "ok" });
        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.DoesNotContain("senha\"", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("teste123", corpo);
    }

    [Fact]
    public async Task Login_sem_captcha_e_recusado()
    {
        var resposta = await _api.CreateClient().PostAsJsonAsync("/auth/login",
            new { email = "juliana@teste.com", senha = "teste123" });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Pedir_recuperacao_nao_derruba_a_senha_atual()
    {
        var anonimo = _api.CreateClient();
        await anonimo.PostAsJsonAsync("/auth/reenviar-senha", new { email = "marlene@teste.com" });

        var resposta = await anonimo.PostAsJsonAsync("/auth/login",
            new { email = "marlene@teste.com", senha = "teste123", recaptchaToken = "ok" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Upload_de_html_disfarcado_de_imagem_e_recusado()
    {
        var (cliente, id) = await _api.ComoAsync("doador@teste.com");

        var arquivo = new ByteArrayContent("<html><script>alert(1)</script></html>"u8.ToArray());
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        var form = new MultipartFormDataContent { { arquivo, "foto", "xss.html" } };

        var resposta = await cliente.PostAsync($"/auth/perfil/{id}/foto", form);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_nao_permite_virar_admin()
    {
        var resposta = await _api.CreateClient().PostAsJsonAsync("/auth/register", new
        {
            nome = "Esperto",
            email = $"esperto{Guid.NewGuid():N}@teste.com",
            senha = "123456",
            tipo = "admin"
        });
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("usuario", json.GetProperty("usuario").GetProperty("tipo").GetString());
    }

    [Fact]
    public async Task Validacao_de_dto_devolve_message()
    {
        var resposta = await _api.CreateClient().PostAsJsonAsync("/auth/register",
            new { nome = "X", email = "nao-e-email", senha = "123456" });
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("E-mail inválido", json.GetProperty("message").GetString());
    }

    private static async Task<int> CriarCampanhaAsync(HttpClient cliente)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/campanhas", new
        {
            titulo = "Campanha de teste",
            descricao = "Descrição",
            metaArrecadacao = 1000,
            dataInicio = DateTime.UtcNow,
            dataFim = DateTime.UtcNow.AddDays(30)
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static object Edicao(string? status) => new
    {
        titulo = "Campanha editada",
        descricao = "Descrição",
        metaArrecadacao = 1000,
        dataInicio = DateTime.UtcNow,
        dataFim = DateTime.UtcNow.AddDays(30),
        status
    };

    [Fact]
    public async Task Dono_nao_ativa_a_propria_campanha_sem_moderacao()
    {
        var (dono, _) = await _api.ComoAsync("doador@teste.com");
        var id = await CriarCampanhaAsync(dono);

        var resposta = await dono.PutAsJsonAsync($"/api/campanhas/{id}", Edicao("ativa"));

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Edicao_aplica_o_status_e_finalizar_encerra()
    {
        var (dono, _) = await _api.ComoAsync("doador@teste.com");
        var (moderador, _) = await _api.ComoAsync("moderador@teste.com");
        var id = await CriarCampanhaAsync(dono);

        Assert.Equal(HttpStatusCode.NoContent, (await dono.PutAsJsonAsync($"/api/campanhas/{id}", Edicao("pausada"))).StatusCode);
        var pausada = await _api.CreateClient().GetFromJsonAsync<JsonElement>($"/api/campanhas/{id}");
        Assert.Equal("pausada", pausada.GetProperty("status").GetString());
        Assert.Equal("Campanha editada", pausada.GetProperty("titulo").GetString());

        Assert.Equal(HttpStatusCode.OK, (await moderador.PostAsync($"/api/campanhas/{id}/finalizar", null)).StatusCode);
        var finalizada = await _api.CreateClient().GetFromJsonAsync<JsonElement>($"/api/campanhas/{id}");
        Assert.Equal("finalizada", finalizada.GetProperty("status").GetString());
        Assert.False(finalizada.GetProperty("ativa").GetBoolean());
    }

    [Fact]
    public async Task Outro_usuario_nao_edita_campanha_alheia()
    {
        var (dono, _) = await _api.ComoAsync("doador@teste.com");
        var (outro, _) = await _api.ComoAsync("juliana@teste.com");
        var id = await CriarCampanhaAsync(dono);

        var resposta = await outro.PutAsJsonAsync($"/api/campanhas/{id}", Edicao(null));

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Doacao_fica_registrada_e_aparece_no_historico()
    {
        var (doador, _) = await _api.ComoAsync("juliana@teste.com");
        var campanhas = await _api.CreateClient().GetFromJsonAsync<JsonElement>("/api/campanhas");
        var campanha = campanhas.GetProperty("data")[0];
        var id = campanha.GetProperty("id").GetInt32();
        var antes = campanha.GetProperty("valorArrecadado").GetDecimal();

        var resposta = await doador.PostAsJsonAsync($"/api/campanhas/{id}/doacoes", new { valor = 37.5m, metodoPagamento = "pix" });
        resposta.EnsureSuccessStatusCode();

        var depois = await _api.CreateClient().GetFromJsonAsync<JsonElement>($"/api/campanhas/{id}");
        Assert.Equal(antes + 37.5m, depois.GetProperty("valorArrecadado").GetDecimal());

        var minhas = await doador.GetFromJsonAsync<JsonElement>("/api/doacoes/minhas");
        Assert.Contains(minhas.GetProperty("data").EnumerateArray(), d =>
            d.GetProperty("campanhaId").GetInt32() == id && d.GetProperty("valor").GetDecimal() == 37.5m);

        // Doar inscreve como participante, o que libera o chat da campanha.
        Assert.Equal(HttpStatusCode.OK, (await doador.GetAsync($"/api/campanhas/{id}/chat")).StatusCode);
    }

    [Fact]
    public async Task Doacao_com_valor_invalido_e_recusada()
    {
        var (doador, _) = await _api.ComoAsync("juliana@teste.com");
        var campanhas = await _api.CreateClient().GetFromJsonAsync<JsonElement>("/api/campanhas");
        var id = campanhas.GetProperty("data")[0].GetProperty("id").GetInt32();

        var resposta = await doador.PostAsJsonAsync($"/api/campanhas/{id}/doacoes", new { valor = -10 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Inscricao_como_voluntario_aparece_no_historico()
    {
        var (voluntario, _) = await _api.ComoAsync("marlene@teste.com");
        var campanhas = await _api.CreateClient().GetFromJsonAsync<JsonElement>("/api/campanhas");
        var id = campanhas.GetProperty("data")[1].GetProperty("id").GetInt32();

        await voluntario.PostAsJsonAsync($"/api/campanhas/{id}/participantes", new { tipo = "voluntario" });
        var minhas = await voluntario.GetFromJsonAsync<JsonElement>("/api/participacoes/minhas");

        Assert.Contains(minhas.GetProperty("data").EnumerateArray(), p => p.GetProperty("campanhaId").GetInt32() == id);
    }
}
