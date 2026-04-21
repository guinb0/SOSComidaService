using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSwaggerGen();

// Configuração de CORS para permitir o frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
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

app.UseAuthorization();

app.MapControllers();

app.Run();
