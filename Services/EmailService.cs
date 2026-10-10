using System.Net;
using System.Net.Mail;
using SOSComida.DTOs.Requests;

namespace SOSComida.Services;

public interface IEmailService
{
    Task<bool> EnviarEmailBoasVindas(string email, string nome, string senhaTemporaria);
    Task<bool> EnviarSenhaRecuperacao(string email, string nome, string senhaTemporaria, TimeSpan validade);
    Task<bool> EnviarEmail(string destinatario, string assunto, string corpo);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
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
            <div class='logo'>🍽️ SOS Comida</div>
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

    public Task<bool> EnviarSenhaRecuperacao(string email, string nome, string senhaTemporaria, TimeSpan validade)
    {
        var corpo = $@"<p>Olá, {WebUtility.HtmlEncode(nome)}.</p>
<p>Recebemos um pedido para recuperar o acesso à sua conta no SOS Comida.</p>
<p>Senha temporária: <strong>{senhaTemporaria}</strong></p>
<p>Ela vale por {validade.TotalMinutes:0} minutos e você vai precisar criar uma senha nova ao entrar.
Sua senha atual continua funcionando — se não foi você que pediu, é só ignorar este e-mail.</p>";

        return EnviarEmail(email, "SOS Comida - Recuperação de acesso", corpo);
    }

    public async Task<bool> EnviarEmail(string destinatario, string assunto, string corpo)
    {
        try
        {
            // Se não houver configuração de email, simular envio
            if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || 
                string.IsNullOrEmpty(_emailSettings.SenderPassword))
            {
                _logger.LogWarning("Email não configurado. Simulando envio para: {Destinatario}", destinatario);
                _logger.LogInformation("=== EMAIL SIMULADO ===");
                _logger.LogInformation("Para: {Destinatario}", destinatario);
                _logger.LogInformation("Assunto: {Assunto}", assunto);
                _logger.LogInformation("======================");
                return true;
            }

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
            _logger.LogInformation("Email enviado com sucesso para: {Destinatario}", destinatario);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para: {Destinatario}", destinatario);
            return false;
        }
    }
}

/// <summary>
/// Serviço fake para desenvolvimento sem SMTP: só escreve no log do console.
/// Antes também gravava as senhas temporárias em Data/emails_enviados.txt,
/// e esse arquivo acabou commitado com senhas reais.
/// </summary>
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;

    public FakeEmailService(ILogger<FakeEmailService> logger) => _logger = logger;

    public Task<bool> EnviarEmailBoasVindas(string email, string nome, string senhaTemporaria)
    {
        _logger.LogInformation("[email simulado] Boas-vindas para {Email} ({Nome}). Senha temporária: {Senha}",
            email, nome, senhaTemporaria);
        return Task.FromResult(true);
    }

    public Task<bool> EnviarSenhaRecuperacao(string email, string nome, string senhaTemporaria, TimeSpan validade)
    {
        _logger.LogInformation("[email simulado] Recuperação para {Email} ({Nome}). Senha temporária: {Senha} (vale {Minutos} min)",
            email, nome, senhaTemporaria, validade.TotalMinutes);
        return Task.FromResult(true);
    }

    public Task<bool> EnviarEmail(string destinatario, string assunto, string corpo)
    {
        _logger.LogInformation("[email simulado] Para {Destinatario}: {Assunto}", destinatario, assunto);
        return Task.FromResult(true);
    }
}
