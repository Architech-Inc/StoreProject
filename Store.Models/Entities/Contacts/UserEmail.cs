using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class UserEmail : BaseEntity
{
    public int UserEmailId { get; set; }
    public Guid UserId { get; set; }
    public int EmailId { get; set; }
    public bool IsPrimary { get; set; }

    public User User { get; set; } = null!;
    public Email Email { get; set; } = null!;
}
