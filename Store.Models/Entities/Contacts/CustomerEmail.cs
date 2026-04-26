using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class CustomerEmail : BaseEntity
{
    public int CustomerEmailId { get; set; }
    public Guid CustomerId { get; set; }
    public int EmailId { get; set; }
    public bool IsPrimary { get; set; }

    public Customer Customer { get; set; } = null!;
    public Email Email { get; set; } = null!;
}
