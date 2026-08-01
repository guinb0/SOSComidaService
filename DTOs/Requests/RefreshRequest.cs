namespace SOSComida.DTOs.Requests;

/// <summary>Corpo de POST /auth/refresh e /auth/logout.</summary>
public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
