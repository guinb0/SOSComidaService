namespace SOSComida.DTOs.Requests;

public class EmailSettings
{
    public string Provider { get; set; } = "resend";
    public string ResendApiKey { get; set; } = string.Empty;
    public string ResendApiUrl { get; set; } = "https://api.resend.com/emails";
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SenderName { get; set; } = "SOS Comida";
    public string LogoUrl { get; set; } = string.Empty;
}
