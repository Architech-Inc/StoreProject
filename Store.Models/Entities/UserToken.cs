using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>
/// Stores the current access token and refresh token for a user.
/// One row per user (updated on each login/refresh).
/// The JWT payload contains only non-sensitive claims (UserId, Role, jti).
/// </summary>
public class UserToken : BaseEntity
{
    public int UserTokenId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>JWT access token string (opaque to the server after issuance).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Cryptographically random refresh token — stored as SHA-256 hash in DB.</summary>
    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
    public DateTime RefreshTokenExpiryDate { get; set; }
    public bool IsRevoked { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
