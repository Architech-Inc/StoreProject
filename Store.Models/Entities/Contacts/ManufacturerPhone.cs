using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class ManufacturerPhone : BaseEntity
{
    public int ManufacturerPhoneId { get; set; }
    public Guid ManufacturerId { get; set; }
    public int PhoneId { get; set; }
    public bool IsPrimary { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;
    public Phone Phone { get; set; } = null!;
}
