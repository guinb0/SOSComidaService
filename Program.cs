using Microsoft.EntityFrameworkCore;
using SOSComida.Data;
using SOSComida.Services;
using SOSComida.DTOs.Requests;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Banco local em arquivo (SQLite).
//
// Não precisa de Docker, container nem porta: o banco é um .db ao lado do
// executável. `dotnet run` sobe a aplicação inteira, e nada morre quando a
// máquina reinicia. Para voltar ao Postgres, troque por UseNpgsql e a
// connection string — o modelo não depende de provider.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do serviço de Email
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);

// Usar FakeEmailService para desenvolvimento (simula envio de email)
builder.Services.AddScoped<IEmailService, FakeEmailService>();

// OCR das notas fiscais da prestação de contas.
// Singleton porque carregar o modelo de idioma do Tesseract é caro; o engine
// é reaproveitado entre requisições. Ele NÃO é thread-safe, então as leituras
// são serializadas dentro do serviço.
builder.Services.AddSingleton<IOcrService, TesseractOcrService>();

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
        // O app Android carrega o front pela WebView, então a origem não é
        // "localhost": no emulador é 10.0.2.2 (alias do host) e num celular
        // físico é o IP da máquina na LAN. Sem essas origens o preflight é
        // barrado e o login falha sem nem chegar no servidor.
        policy => policy.WithOrigins(
                            "http://localhost:3000",
                            "http://10.0.2.2:3000",
                            "http://192.168.5.241:3000")
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
