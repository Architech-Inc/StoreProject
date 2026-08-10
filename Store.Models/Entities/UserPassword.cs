using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>
/// Stores a BCrypt hash of the user's password.
/// No plain-text or reversibly-encrypted passwords are stored.
/// </summary>
public class UserPassword : BaseEntity
{
    public int UserPasswordId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>BCrypt hash (includes salt). Never store plain-text here.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
