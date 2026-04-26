using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class SupplierEmail : BaseEntity
{
    public int SupplierEmailId { get; set; }
    public Guid SupplierId { get; set; }
    public int EmailId { get; set; }
    public bool IsPrimary { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public Email Email { get; set; } = null!;
}
