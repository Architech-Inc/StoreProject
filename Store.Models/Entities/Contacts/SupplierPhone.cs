using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class SupplierPhone : BaseEntity
{
    public int SupplierPhoneId { get; set; }
    public Guid SupplierId { get; set; }
    public int PhoneId { get; set; }
    public bool IsPrimary { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public Phone Phone { get; set; } = null!;
}
