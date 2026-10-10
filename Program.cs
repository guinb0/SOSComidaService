using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SOSComida.Data;
using SOSComida.DTOs.Requests;
using SOSComida.Seguranca;
using SOSComida.Services;

var builder = WebApplication.CreateBuilder(args);

// Segredos locais (chave do OCR, SMTP...) ficam em appsettings.Local.json,
// que está no .gitignore. Variáveis de ambiente continuam valendo por cima.
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Banco local em arquivo (SQLite).
//
// Não precisa de Docker, container nem porta: o banco é um .db ao lado do
// executável. `dotnet run` sobe a aplicação inteira, e nada morre quando a
// máquina reinicia. Para voltar ao Postgres, troque por UseNpgsql e a
// connection string — o modelo não depende de provider.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Email: com remetente configurado envia de verdade; sem ele, só registra
// no log (FakeEmailService) para o fluxo funcionar em desenvolvimento.
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
if (string.IsNullOrWhiteSpace(emailSettings.SenderEmail))
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
else
    builder.Services.AddScoped<IEmailService, EmailService>();

// OCR das notas fiscais da prestação de contas. Com Ocr:ChaveApi (variável
// Ocr__ChaveApi) usa a API da Mistral; sem chave, o Tesseract local.
// O Tesseract é singleton porque carregar o modelo de idioma é caro; o engine
// não é thread-safe, então as leituras são serializadas dentro do serviço.
var ocr = builder.Configuration.GetSection("Ocr").Get<OcrSettings>() ?? new OcrSettings();
builder.Services.AddSingleton(ocr);
if (ocr.UsaMistral)
    builder.Services.AddHttpClient<IOcrService, MistralOcrService>(c =>
        c.Timeout = TimeSpan.FromSeconds(ocr.TimeoutSegundos));
else
    builder.Services.AddSingleton<IOcrService, TesseractOcrService>();

builder.Services.AddSingleton(builder.Configuration.GetSection("Recaptcha").Get<RecaptchaSettings>() ?? new RecaptchaSettings());
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();

// Autenticação: JWT assinado pela API. O id e o tipo do usuário saem do
// token — nenhum endpoint confia mais em usuarioId/moderadorId do cliente.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
jwt.Validar(builder.Environment.IsDevelopment());
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Emissor,
            ValidAudience = jwt.Audiencia,
            IssuerSigningKey = jwt.ChaveDeAssinatura(),
            NameClaimType = TokenService.ClaimNome,
            RoleClaimType = TokenService.ClaimPapel,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // O token vale horas; sem esta checagem, desativar uma conta ou tirar
        // alguém da moderação só teria efeito quando ele expirasse.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async contexto =>
            {
                var db = contexto.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var id = contexto.Principal?.IdOuNulo();
                var papel = contexto.Principal?.FindFirst(TokenService.ClaimPapel)?.Value;

                var atual = id == null ? null : await db.Usuarios.AsNoTracking()
                    .Where(u => u.Id == id)
                    .Select(u => new { u.Ativo, u.Tipo })
                    .FirstOrDefaultAsync();

                if (atual == null || !atual.Ativo || atual.Tipo != papel)
                    contexto.Fail("Sessão não é mais válida.");
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Tudo exige login, a não ser o que está marcado com [AllowAnonymous].
    // Endpoint novo nasce fechado — esquecer o atributo não abre nada.
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy(Politicas.Moderacao, p => p.RequireRole(TiposUsuario.Moderador, TiposUsuario.Admin));
    options.AddPolicy(Politicas.Admin, p => p.RequireRole(TiposUsuario.Admin));
});

var tentativasPorMinuto = builder.Configuration.GetValue("LimitesDeTaxa:AutenticacaoPorMinuto", 10);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(LimitesDeTaxa.Autenticacao, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = tentativasPorMinuto, Window = TimeSpan.FromMinutes(1) }));
    options.OnRejected = async (contexto, token) =>
        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Muitas tentativas. Aguarde um minuto e tente de novo." }, token);
});

builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Aceitar propriedades JSON case-insensitive (camelCase do JS para PascalCase do C#)
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // As telas leem `message` do corpo do erro. Sem isto a validação dos
        // DTOs voltaria no formato ProblemDetails e o usuário veria um erro
        // genérico em vez de "E-mail inválido".
        options.InvalidModelStateResponseFactory = contexto =>
        {
            var erros = contexto.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            var primeiro = erros.Values.SelectMany(x => x).FirstOrDefault(m => !string.IsNullOrEmpty(m));
            return new BadRequestObjectResult(new { message = primeiro ?? "Dados inválidos", errors = erros });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token devolvido por POST /auth/login"
    });
    c.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
    });
});

// Origens permitidas vêm de Cors:Origens. O app Android carrega o front pela
// WebView, então além de localhost entram 10.0.2.2 (emulador) e o IP da
// máquina na LAN (celular físico) — ver appsettings.Development.json.
var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(origens)
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Cria/migra o banco e, se estiver vazio, popula com o cenário de demonstração.
// Roda no arranque porque com SQLite não há um passo de "subir o servidor de
// banco" — o arquivo é criado aqui e o app já sobe utilizável.
using (var escopo = app.Services.CreateScope())
{
    var ctx = escopo.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedInicial.ExecutarAsync(ctx);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();

// Servir arquivos estáticos (fotos de perfil, notas). O nosniff impede o
// navegador de "adivinhar" que um upload é HTML/JS e executá-lo.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
});

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program;
