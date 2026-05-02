using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Item : BaseEntity
{
    public Guid ItemId { get; set; }
    public int? CategoryId { get; set; }
    public int? UnitId { get; set; }
    public Guid? ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int InStock { get; set; }
    public int? ReorderLevel { get; set; }
    public ItemType Type { get; set; } = ItemType.Product;
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImagePath { get; set; }
    public int? TaxProfileId { get; set; }

    // Navigation
    public Category? Category { get; set; }
    public Unit? Unit { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public ItemExpiry? ItemExpiry { get; set; }
    public Discount? Discount { get; set; }
    public TaxProfile? TaxProfile { get; set; }

    public ICollection<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<CustomerSegmentPrice> SegmentPrices { get; set; } = new List<CustomerSegmentPrice>();
    public ICollection<ItemCode> ItemCodes { get; set; } = new List<ItemCode>();
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
