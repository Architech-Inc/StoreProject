namespace Store.Models.DTOs.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public bool RequiresPasswordReset { get; set; }
    public bool IsLockedOut { get; set; }
    public int LockoutRemainingMinutes { get; set; }
    public AuthenticatedUserDto User { get; set; } = null!;
}

public class AuthenticatedUserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
}
