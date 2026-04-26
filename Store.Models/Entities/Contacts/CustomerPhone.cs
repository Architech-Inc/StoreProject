using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class CustomerPhone : BaseEntity
{
    public int CustomerPhoneId { get; set; }
    public Guid CustomerId { get; set; }
    public int PhoneId { get; set; }
    public bool IsPrimary { get; set; }

    public Customer Customer { get; set; } = null!;
    public Phone Phone { get; set; } = null!;
}
