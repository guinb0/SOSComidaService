/*
Exemplo de configuração para Program.cs:

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Adicionar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://soscomida.vercel.app" // Adicionar seu domínio de produção
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configurar JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
            ClockSkew = TimeSpan.Zero // Opcional: remover tolerância padrão de 5 minutos
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar serviços aqui
// builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
// builder.Services.AddScoped<ICampanhaService, CampanhaService>();
// builder.Services.AddScoped<JwtService>();

// Configure o pipeline conforme necessário
// ...
*/
/*
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// EXEMPLO: Copie o conteúdo relevante para dentro do método Main ou para o arquivo Program.cs
// Não utilize instruções de nível superior neste arquivo.
// ...código comentado...
*/
