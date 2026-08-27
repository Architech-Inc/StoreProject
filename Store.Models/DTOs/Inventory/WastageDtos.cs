using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Inventory;

public class WastageEntryDto
{
    public int WastageEntryId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string WastageType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineValuation => Quantity * UnitCost;
    public int InStock { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceCode { get; set; }
    public string RecordedByUser { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}

public class WastageMetricsDto
{
    public int TotalEntries { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalValuationXaf { get; set; }
    public decimal TotalExpiredLossXaf { get; set; }
    public decimal TotalDamagedLossXaf { get; set; }
    public decimal TotalSpoiledLossXaf { get; set; }
    public decimal TotalTheftLossXaf { get; set; }
}

public class WastageFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? WastageType { get; set; }
    public Guid? ItemId { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class RecordWastageRequest
{
    [Required]
    public Guid ItemId { get; set; }

    public WastageType WastageType { get; set; } = WastageType.Damage;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? ReferenceCode { get; set; }
}
