using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class ManufacturerEmail : BaseEntity
{
    public int ManufacturerEmailId { get; set; }
    public Guid ManufacturerId { get; set; }
    public int EmailId { get; set; }
    public bool IsPrimary { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;
    public Email Email { get; set; } = null!;
}
