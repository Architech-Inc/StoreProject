using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class PasswordResetToken : BaseEntity
{
    public int PasswordResetTokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }

    public User User { get; set; } = null!;
}
