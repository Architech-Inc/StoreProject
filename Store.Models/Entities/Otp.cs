using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Otp : BaseEntity
{
    public int OtpId { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public User User { get; set; } = null!;
}
