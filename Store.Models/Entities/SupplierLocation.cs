using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class SupplierLocation : BaseEntity
{
    public int SupplierLocationId { get; set; }
    public Guid SupplierId { get; set; }
    public int LocationId { get; set; }
    public bool IsPrimary { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
