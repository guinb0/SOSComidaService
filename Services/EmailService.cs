using System.Net;
using System.Net.Mail;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SOSComida.DTOs.Requests;

namespace SOSComida.Services;

public interface IEmailService
{
    Task<bool> EnviarEmailBoasVindas(string email, string nome, string senhaTemporaria);
    Task<bool> EnviarEmailRecuperacaoSenha(string email, string nome, string senhaTemporaria);
    Task<bool> EnviarEmail(string destinatario, string assunto, string corpo);
}

public class EmailService : IEmailService
{
    private static readonly HttpClient HttpClient = new();
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }

    private string GetLogoMarkup()
    {
        if (!string.IsNullOrWhiteSpace(_emailSettings.LogoUrl))
        {
            return $"<img src='{_emailSettings.LogoUrl}' alt='Logo SOS Comida' style='max-width:180px;height:auto;display:block;margin:0 auto 8px auto;' />";
        }

        return @"
            <div style='display:flex;justify-content:center;margin-bottom:8px;'>
                <div style='display:flex;align-items:center;gap:8px;padding:8px 14px;border:1px solid #475569;border-radius:999px;background:#0f172a;'>
                    <span style='font-size:20px;line-height:1;'>🍽️</span>
                    <span class='logo'>SOS Comida</span>
                </div>
            </div>";
    }

    public async Task<bool> EnviarEmailBoasVindas(string email, string nome, string senhaTemporaria)
    {
        var assunto = "🎉 Bem-vindo ao SOS Comida - Sua conta foi criada!";
        
        var corpo = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #0f172a; color: #e2e8f0; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #1e293b 0%, #334155 100%); border-radius: 16px; padding: 40px; border: 1px solid #475569; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 32px; font-weight: bold; background: linear-gradient(90deg, #10b981, #3b82f6); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }}
        .content {{ background-color: #1e293b; border-radius: 12px; padding: 30px; margin: 20px 0; border: 1px solid #475569; }}
        .credentials {{ background: linear-gradient(135deg, #065f46 0%, #1e40af 100%); border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .credential-item {{ margin: 10px 0; }}
        .label {{ color: #94a3b8; font-size: 14px; }}
        .value {{ color: #ffffff; font-size: 18px; font-weight: bold; font-family: monospace; background-color: rgba(0,0,0,0.3); padding: 8px 12px; border-radius: 6px; display: inline-block; margin-top: 5px; }}
        .warning {{ background-color: #7c2d12; border: 1px solid #ea580c; border-radius: 8px; padding: 15px; margin: 20px 0; color: #fed7aa; }}
        .button {{ display: inline-block; background: linear-gradient(90deg, #10b981, #3b82f6); color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #64748b; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            {GetLogoMarkup()}
            <p style='color: #94a3b8;'>Juntos contra a fome</p>
        </div>
        
        <h2 style='color: #10b981;'>Olá, {nome}! 👋</h2>
        
        <p>Sua conta no <strong>SOS Comida</strong> foi criada com sucesso! Estamos muito felizes em ter você conosco nessa missão de combater a fome.</p>
        
        <div class='content'>
            <h3 style='color: #3b82f6; margin-top: 0;'>📧 Suas Credenciais de Acesso</h3>
            
            <div class='credentials'>
                <div class='credential-item'>
                    <div class='label'>Email de Login:</div>
                    <div class='value'>{email}</div>
                </div>
                <div class='credential-item'>
                    <div class='label'>Senha Temporária:</div>
                    <div class='value'>{senhaTemporaria}</div>
                </div>
            </div>
            
            <div class='warning'>
                ⚠️ <strong>Importante:</strong> Esta é uma senha temporária. Por segurança, recomendamos que você altere sua senha no primeiro acesso.
            </div>
        </div>
        
        <div style='text-align: center;'>
            <a href='http://localhost:3000/login' class='button'>Acessar Minha Conta</a>
        </div>
        
        <div class='content'>
            <h3 style='color: #a855f7; margin-top: 0;'>🌟 O que você pode fazer agora:</h3>
            <ul style='color: #cbd5e1;'>
                <li>✅ Explorar campanhas ativas de arrecadação</li>
                <li>✅ Fazer doações para ajudar famílias necessitadas</li>
                <li>✅ Se voluntariar em campanhas da sua região</li>
                <li>✅ Acompanhar o impacto das suas contribuições</li>
            </ul>
        </div>
        
        <div class='footer'>
            <p>Este email foi enviado automaticamente. Por favor, não responda.</p>
            <p>© 2025 SOS Comida - Todos os direitos reservados</p>
            <p style='margin-top: 15px;'>
                <a href='#' style='color: #10b981; text-decoration: none;'>Precisa de ajuda?</a> | 
                <a href='#' style='color: #10b981; text-decoration: none;'>Política de Privacidade</a>
            </p>
        </div>
    </div>
</body>
</html>";

        return await EnviarEmail(email, assunto, corpo);
    }

    public async Task<bool> EnviarEmailRecuperacaoSenha(string email, string nome, string senhaTemporaria)
    {
        var assunto = "🔐 SOS Comida - Recuperação de senha";

        var corpo = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #0f172a; color: #e2e8f0; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #1e293b 0%, #334155 100%); border-radius: 16px; padding: 32px; border: 1px solid #475569; }}
        .header {{ text-align: center; margin-bottom: 24px; }}
        .logo {{ font-size: 28px; font-weight: bold; color: #10b981; }}
        .card {{ background-color: #1e293b; border-radius: 12px; padding: 24px; margin: 20px 0; border: 1px solid #475569; }}
        .senha {{ color: #ffffff; font-size: 20px; font-weight: bold; font-family: monospace; background-color: rgba(0,0,0,0.3); padding: 10px 14px; border-radius: 6px; display: inline-block; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            {GetLogoMarkup()}
            <p style='color: #94a3b8; margin: 0;'>Recuperação de acesso</p>
        </div>
        <h2>Olá, {nome}!</h2>
        <p>Recebemos uma solicitação de recuperação de senha para sua conta no <strong>SOS Comida</strong>.</p>

        <div class='card'>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Senha temporária:</strong></p>
            <div class='senha'>{senhaTemporaria}</div>
            <p>Após entrar, altere sua senha imediatamente em seu perfil.</p>
        </div>

        <p>Se você não solicitou esta alteração, entre em contato com o suporte.</p>
        <p style='color:#94a3b8;font-size:12px;'>Este email foi enviado automaticamente. Não responda.</p>
    </div>
</body>
</html>";

        return await EnviarEmail(email, assunto, corpo);
    }

    public async Task<bool> EnviarEmail(string destinatario, string assunto, string corpo)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_emailSettings.ResendApiKey))
            {
                return await EnviarViaResend(destinatario, assunto, corpo);
            }

            if (!string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) &&
                !string.IsNullOrWhiteSpace(_emailSettings.SenderPassword))
            {
                return await EnviarViaSmtp(destinatario, assunto, corpo);
            }

            // Se não houver configuração de email, simular envio
            if (string.IsNullOrEmpty(_emailSettings.SenderEmail))
            {
                _logger.LogWarning("Email não configurado. Simulando envio para: {Destinatario}", destinatario);
                _logger.LogInformation("=== EMAIL SIMULADO ===");
                _logger.LogInformation("Para: {Destinatario}", destinatario);
                _logger.LogInformation("Assunto: {Assunto}", assunto);
                _logger.LogInformation("======================");
                
                // Salvar email em arquivo para debug
                var emailLogPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "emails_enviados.txt");
                var logEntry = $"{DateTime.Now}|{destinatario}|{assunto}\n";
                await File.AppendAllTextAsync(emailLogPath, logEntry);
                
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para: {Destinatario}", destinatario);
            return false;
        }
    }

    private async Task<bool> EnviarViaSmtp(string destinatario, string assunto, string corpo)
    {
        using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = assunto,
            Body = corpo,
            IsBodyHtml = true
        };
        mailMessage.To.Add(destinatario);

        await client.SendMailAsync(mailMessage);
        _logger.LogInformation("Email SMTP enviado com sucesso para: {Destinatario}", destinatario);
        return true;
    }

    private async Task<bool> EnviarViaResend(string destinatario, string assunto, string corpo)
    {
        var from = string.IsNullOrWhiteSpace(_emailSettings.SenderName)
            ? _emailSettings.SenderEmail
            : $"{_emailSettings.SenderName} <{_emailSettings.SenderEmail}>";

        if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
        {
            _logger.LogError("SenderEmail não configurado para envio via Resend.");
            return false;
        }

        var request = new ResendEmailRequest(
            from,
            new[] { destinatario },
            assunto,
            corpo
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, _emailSettings.ResendApiUrl);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _emailSettings.ResendApiKey);
        message.Content = JsonContent.Create(request);

        using var response = await HttpClient.SendAsync(message);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email Resend enviado com sucesso para: {Destinatario}", destinatario);
            return true;
        }

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogError("Falha ao enviar email via Resend ({StatusCode}): {Body}", (int)response.StatusCode, body);
        return false;
    }

    private sealed record ResendEmailRequest(
        string from,
        string[] to,
        string subject,
        string html
    );
}

// Serviço fake para desenvolvimento sem SMTP
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;
    private readonly string _emailLogPath;

    public FakeEmailService(ILogger<FakeEmailService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _emailLogPath = Path.Combine(env.ContentRootPath, "Data", "emails_enviados.txt");
    }

    public async Task<bool> EnviarEmailBoasVindas(string email, string nome, string senhaTemporaria)
    {
        _logger.LogInformation("=== 📧 EMAIL DE BOAS-VINDAS (SIMULADO) ===");
        _logger.LogInformation("Para: {Email}", email);
        _logger.LogInformation("Nome: {Nome}", nome);
        _logger.LogInformation("Senha Temporária: {Senha}", senhaTemporaria);
        _logger.LogInformation("==========================================");

        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|BOAS_VINDAS|{email}|{nome}|{senhaTemporaria}\n";
        await File.AppendAllTextAsync(_emailLogPath, logEntry);

        return true;
    }

    public async Task<bool> EnviarEmailRecuperacaoSenha(string email, string nome, string senhaTemporaria)
    {
        _logger.LogInformation("=== 🔐 EMAIL RECUPERACAO (SIMULADO) ===");
        _logger.LogInformation("Para: {Email}", email);
        _logger.LogInformation("Nome: {Nome}", nome);
        _logger.LogInformation("Senha Temporária: {Senha}", senhaTemporaria);
        _logger.LogInformation("=======================================");

        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|RECUPERACAO|{email}|{nome}|{senhaTemporaria}\n";
        await File.AppendAllTextAsync(_emailLogPath, logEntry);

        return true;
    }

    public async Task<bool> EnviarEmail(string destinatario, string assunto, string corpo)
    {
        _logger.LogInformation("=== 📧 EMAIL (SIMULADO) ===");
        _logger.LogInformation("Para: {Destinatario}", destinatario);
        _logger.LogInformation("Assunto: {Assunto}", assunto);
        _logger.LogInformation("===========================");

        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|GERAL|{destinatario}|{assunto}\n";
        await File.AppendAllTextAsync(_emailLogPath, logEntry);

        return true;
    }
}
