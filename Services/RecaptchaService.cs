using System.Text.Json.Serialization;

namespace SOSComida.Services;

public class RecaptchaSettings
{
    public string SecretKey { get; set; } = string.Empty;
}

public interface IRecaptchaService
{
    Task<bool> ValidarAsync(string? token, string? ip);
}

/// <summary>
/// Confere o token do reCAPTCHA com o Google. Antes o captcha existia só na
/// tela de login: a API nunca olhava o token, então bastava chamar
/// /auth/login direto para pular o desafio.
///
/// Sem SecretKey configurada a validação falha — nunca libera por omissão.
/// O appsettings.Development.json usa a chave de teste pública do Google,
/// que aceita qualquer token da site key de teste.
/// </summary>
public class RecaptchaService : IRecaptchaService
{
    private readonly HttpClient _http;
    private readonly RecaptchaSettings _settings;
    private readonly ILogger<RecaptchaService> _logger;

    public RecaptchaService(HttpClient http, RecaptchaSettings settings, ILogger<RecaptchaService> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> ValidarAsync(string? token, string? ip)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            _logger.LogError("Recaptcha:SecretKey não configurada; login recusado.");
            return false;
        }

        var campos = new Dictionary<string, string>
        {
            ["secret"] = _settings.SecretKey,
            ["response"] = token
        };
        if (!string.IsNullOrEmpty(ip)) campos["remoteip"] = ip;

        try
        {
            using var resposta = await _http.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(campos));
            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaGoogle>();
            return corpo?.Success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao validar reCAPTCHA");
            return false;
        }
    }

    private sealed class RespostaGoogle
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
