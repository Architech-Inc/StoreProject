using Store.Models.Entities.Base;
using Store.Models.Entities.Contacts;

namespace Store.Models.Entities;

public class Supplier : BaseEntity
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }

    // Navigation
    public ICollection<SupplierEmail> Emails { get; set; } = new List<SupplierEmail>();
    public ICollection<SupplierPhone> Phones { get; set; } = new List<SupplierPhone>();
    public ICollection<SupplierLocation> Locations { get; set; } = new List<SupplierLocation>();
    public ICollection<ItemsOrder> Orders { get; set; } = new List<ItemsOrder>();
}
