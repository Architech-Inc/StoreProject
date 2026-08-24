namespace Store.Models.DTOs.Auth;

public class UserSessionDto
{
    public int SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastActive { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsCurrentSession { get; set; }
}
