using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Services;
using SOSComida.DTOs.Requests;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configuração do Entity Framework com PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do serviço de Email
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);

// Usar FakeEmailService para desenvolvimento (simula envio de email)
builder.Services.AddScoped<IEmailService, FakeEmailService>();

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
