using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class UserPhone : BaseEntity
{
    public int UserPhoneId { get; set; }
    public Guid UserId { get; set; }
    public int PhoneId { get; set; }
    public bool IsPrimary { get; set; }

    public User User { get; set; } = null!;
    public Phone Phone { get; set; } = null!;
}
