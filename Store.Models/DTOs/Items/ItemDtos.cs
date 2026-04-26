using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Items;

public class ItemDto
{
    public Guid ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int InStock { get; set; }
    public int? ReorderLevel { get; set; }
    public ItemType Type { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; }
    public string? ImagePath { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? UnitId { get; set; }
    public string? UnitAbbreviation { get; set; }
    public Guid? ManufacturerId { get; set; }
    public string? ManufacturerName { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime DateCreated { get; set; }
}

public class CreateItemRequest
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CostPrice { get; set; }

    [Required, Range(0, int.MaxValue)]
    public int InStock { get; set; }

    public int? ReorderLevel { get; set; }
    public ItemType Type { get; set; } = ItemType.Product;
    public string? Barcode { get; set; }
    public int? CategoryId { get; set; }
    public int? UnitId { get; set; }
    public Guid? ManufacturerId { get; set; }
}

public class UpdateItemRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CostPrice { get; set; }

    public int? ReorderLevel { get; set; }
    public bool? IsActive { get; set; }
    public string? Barcode { get; set; }
    public ItemType? Type { get; set; }
    public int? CategoryId { get; set; }
    public int? UnitId { get; set; }
    public Guid? ManufacturerId { get; set; }
}

public class AdjustStockRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public int AdjustmentQuantity { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
