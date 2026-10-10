using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SOSComida.Tests;

public class ModeradoresTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _api;

    public ModeradoresTests(ApiFactory api) => _api = api;

    private async Task<(int Id, HttpClient Sessao)> CriarModeradorAsync(HttpClient admin)
    {
        var email = $"mod{Guid.NewGuid():N}@teste.com";
        var criado = await admin.PostAsJsonAsync("/api/moderadores", new { nome = "Novo Mod", email, senha = "segredo1" });
        criado.EnsureSuccessStatusCode();
        var id = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var login = await _api.CreateClient().PostAsJsonAsync("/auth/login", new { email, senha = "segredo1", recaptchaToken = "ok" });
        var corpo = await login.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.GetProperty("senhaTemporaria").GetBoolean());

        var sessao = _api.CreateClient();
        sessao.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", corpo.GetProperty("token").GetString());
        return (id, sessao);
    }

    [Fact]
    public async Task So_admin_gerencia_moderadores()
    {
        var (moderador, _) = await _api.ComoAsync("moderador@teste.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await moderador.GetAsync("/api/moderadores")).StatusCode);
    }

    [Fact]
    public async Task Moderador_criado_pelo_admin_entra_e_modera()
    {
        var (admin, _) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        var (_, sessao) = await CriarModeradorAsync(admin);

        Assert.Equal(HttpStatusCode.OK, (await sessao.GetAsync("/api/campanhas/moderacao")).StatusCode);
    }

    [Fact]
    public async Task Desativar_derruba_a_sessao_na_hora()
    {
        var (admin, _) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        var (id, sessao) = await CriarModeradorAsync(admin);

        (await admin.PostAsync($"/api/moderadores/{id}/desativar", null)).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Unauthorized, (await sessao.GetAsync("/api/campanhas/moderacao")).StatusCode);
    }

    [Fact]
    public async Task Remover_da_equipe_tira_o_acesso_de_moderacao()
    {
        var (admin, _) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        var (id, sessao) = await CriarModeradorAsync(admin);

        (await admin.DeleteAsync($"/api/moderadores/{id}")).EnsureSuccessStatusCode();

        // O token antigo dizia "moderador"; o papel mudou, então ele não vale mais.
        Assert.Equal(HttpStatusCode.Unauthorized, (await sessao.GetAsync("/api/campanhas/moderacao")).StatusCode);
        var equipe = await admin.GetFromJsonAsync<JsonElement>("/api/moderadores");
        Assert.DoesNotContain(equipe.GetProperty("data").EnumerateArray(), m => m.GetProperty("id").GetInt32() == id);
    }

    [Fact]
    public async Task Admin_nao_pode_ser_rebaixado_por_aqui()
    {
        var (admin, adminId) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.DeleteAsync($"/api/moderadores/{adminId}")).StatusCode);
    }

    [Fact]
    public async Task Acoes_de_moderacao_entram_no_historico()
    {
        var (admin, _) = await _api.ComoAsync("guinb@soscomida.com", "admin123");
        var (id, _) = await CriarModeradorAsync(admin);
        (await admin.PostAsync($"/api/moderadores/{id}/desativar", null)).EnsureSuccessStatusCode();

        var historico = await admin.GetFromJsonAsync<JsonElement>("/api/moderadores/historico");
        var tipos = historico.GetProperty("data").EnumerateArray()
            .Where(a => a.GetProperty("itemId").GetInt32() == id)
            .Select(a => a.GetProperty("tipoAcao").GetString())
            .ToList();

        Assert.Contains("moderador_criado", tipos);
        Assert.Contains("moderador_desativado", tipos);
    }
}
