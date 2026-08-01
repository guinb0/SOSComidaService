using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
// OpenApi 2.x (Swashbuckle 10): os tipos saíram de Microsoft.OpenApi.Models.
using Microsoft.OpenApi;
using SOSComida.Data;
using SOSComida.Services;
using SOSComida.DTOs.Requests;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        Path.Combine(builder.Environment.ContentRootPath, "appsettings.Development.local.json"),
        optional: true,
        reloadOnChange: true);

    // O arquivo acima entra depois das fontes padrão e, sozinho, passaria a ter a
    // maior precedência — silenciando ConnectionStrings__DefaultConnection e os
    // argumentos de linha de comando prometidos logo abaixo. Reaplicá-los devolve
    // a ordem esperada: local.json < variáveis de ambiente < linha de comando.
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(args);
}

// Add services to the container.

// PostgreSQL (local ou Supabase). Senha: appsettings.Development.local.json (gitignored), User Secrets ou env ConnectionStrings__DefaultConnection.
// Ex.: dotnet user-secrets set ConnectionStrings:DefaultConnection "Host=db.oxrxcrksnxyjzqbffoiv.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true"
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Defina ConnectionStrings:DefaultConnection (appsettings, variável de ambiente ou User Secrets).");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }));

// --- Autenticação ------------------------------------------------------------
// Até aqui a API era ABERTA: nenhum controller tinha [Authorize], nenhum esquema
// estava registrado, e o "token" do login era a string `mock-jwt-token-...`, que
// ninguém validava. Qualquer requisição anônima podia criar, editar e apagar
// campanhas, e chamar os endpoints de moderação.

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwt.Secret))
{
    if (builder.Environment.IsDevelopment())
    {
        // Chave efêmera só para o dev subir sem configurar nada. Muda a cada
        // reinício (os tokens antigos param de valer) — é de propósito: nada de
        // segredo default virar segredo de produção sem querer.
        jwt.Secret = Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        Console.WriteLine("⚠️  Jwt:Secret não definido — usando chave efêmera de desenvolvimento.");
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:Secret é obrigatório fora de Development. Defina a variável de ambiente Jwt__Secret " +
            "com pelo menos 32 bytes aleatórios.");
    }
}

if (Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Secret curto demais: HMAC-SHA256 exige no mínimo 32 bytes.");
}

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<ISenhaService, SenhaService>();
// Scoped: usa o DbContext (que é scoped).
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Quem modera: moderador ou admin. Usado pelos endpoints de moderação.
    options.AddPolicy("Moderacao", p => p.RequireRole("moderador", "admin"));
    options.AddPolicy("Admin", p => p.RequireRole("admin"));
});

// --- Rate limiting no login -------------------------------------------------
// Agora que o login valida senha de verdade, ele virou alvo de força bruta.
// A política "auth" limita as tentativas por IP; passou do teto, responde 429.
// Janela fixa: simples e suficiente para conter um atacante batendo no endpoint.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", contexto =>
    {
        // Chave = IP. Sob proxy/CDN, prefira o X-Forwarded-For encaminhado.
        var ip = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,                    // 10 tentativas...
            Window = TimeSpan.FromMinutes(1),    // ...por minuto, por IP
            QueueLimit = 0,                      // excedeu, rejeita na hora (não enfileira)
        });
    });
});

// Configuração do serviço de Email
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);

var resendConfigurado = !string.IsNullOrWhiteSpace(emailSettings.ResendApiKey) &&
                        !string.IsNullOrWhiteSpace(emailSettings.SenderEmail);
var smtpConfigurado = !string.IsNullOrWhiteSpace(emailSettings.SenderEmail) &&
                      !string.IsNullOrWhiteSpace(emailSettings.SenderPassword);

if (resendConfigurado || smtpConfigurado)
{
    builder.Services.AddScoped<IEmailService, EmailService>();
}
else
{
    // Fallback para desenvolvimento/local quando senha de app do SMTP ainda não foi configurada.
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Aceitar propriedades JSON case-insensitive (camelCase do JS para PascalCase do C#)
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Botão "Authorize" no Swagger, para testar os endpoints protegidos.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole o token devolvido por POST /auth/login (sem o prefixo 'Bearer').",
    });

    // No OpenApi 2.x a referência a um esquema é um tipo próprio
    // (OpenApiSecuritySchemeReference) e o requisito é registrado por uma função
    // que recebe o documento — não mais pelo objeto pronto.
    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() },
    });
});

// CORS. A lista fixa em "http://localhost:3000" barrava o app aberto no celular,
// cuja origem é o IP desta máquina na LAN (ex.: http://192.168.0.63:3000).
// Configure em Cors:AllowedOrigins (appsettings) ou em
// Cors__AllowedOrigins__0 (variável de ambiente).
var origensPermitidas = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(origensPermitidas)
              .AllowAnyHeader()
              .AllowAnyMethod();

        // Em desenvolvimento, aceita também qualquer host da rede local, para não
        // exigir reconfiguração toda vez que o IP do DHCP mudar. Nunca em produção.
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origem =>
                Uri.TryCreate(origem, UriKind.Absolute, out var uri) &&
                (uri.IsLoopback ||
                 uri.Host.StartsWith("192.168.") ||
                 uri.Host.StartsWith("10.") ||
                 uri.Host.StartsWith("172.")));
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Servir arquivos estáticos (fotos de perfil, etc.)
app.UseStaticFiles();

// Aplicar política de CORS
app.UseCors("AllowFrontend");

// UseAuthorization sozinho NÃO protege nada: sem o UseAuthentication antes dele,
// não há quem leia o token e monte o User da requisição. Era esse o buraco.
app.UseAuthentication();
app.UseAuthorization();

// Depois do CORS (senão a resposta 429 volta sem os headers de CORS e o navegador
// esconde o erro real do usuário).
app.UseRateLimiter();

app.MapControllers();

app.Run();
