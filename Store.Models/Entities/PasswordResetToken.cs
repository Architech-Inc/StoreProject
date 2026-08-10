using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class PasswordResetToken : BaseEntity
{
    public int PasswordResetTokenId { get; set; }
    public Guid UserId { get; set; }
    
    /// <summary>
    /// A cryptographically secure random token (stored as SHA-256 hash for security)
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;
    
    public DateTime ExpiryDate { get; set; }
    
    public bool IsUsed { get; set; } = false;

    public User User { get; set; } = null!;
}
