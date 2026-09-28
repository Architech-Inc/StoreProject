using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Otp : BaseEntity
{
    public int OtpId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// HMAC-SHA256 of the raw OTP code, keyed by <c>Auth:OtpPepper</c>. The plaintext
    /// <c>Code</c> column was removed in Wave 12 (SEC-06): the database never sees the
    /// raw code again, and a leaked backup or DBA read access cannot redeem active OTPs.
    /// Verification uses <see cref="System.Security.Cryptography.CryptographicOperations.FixedTimeEquals"/>
    /// to neutralize timing oracles against the comparison.
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public User User { get; set; } = null!;
}
