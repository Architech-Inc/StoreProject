using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Inventory;

public class BatchDto
{
    public Guid BatchId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TotalValuation => Quantity * CostPrice;
    public DateTime ReceivedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string ExpiryStatus { get; set; } = string.Empty; // "OK", "Expiring", "Expired"
}

public class CreateBatchRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Required, StringLength(100)]
    public string BatchNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CostPrice { get; set; }

    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class UpdateBatchRequest
{
    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [Range(0, int.MaxValue)]
    public int? Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CostPrice { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class BatchMetricsDto
{
    public int TotalBatches { get; set; }
    public int TotalExpiring30Days { get; set; }
    public int TotalExpired { get; set; }
    public int TotalTrackedUnits { get; set; }
    public decimal TotalBatchValuationXaf { get; set; }
}

public class BatchFilterRequest : Store.Models.DTOs.Common.PagedRequest
{
    public Guid? ItemId { get; set; }
    public string? ExpiryStatus { get; set; }
    public DateTime? FromExpiry { get; set; }
    public DateTime? ToExpiry { get; set; }
}

public class WriteOffBatchRequest
{
    [Required]
    public Guid BatchId { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public Store.Models.Enums.WastageType WastageType { get; set; } = Store.Models.Enums.WastageType.Expiry;

    [Required, StringLength(200)]
    public string Reason { get; set; } = "Batch expired / damaged";

    [StringLength(500)]
    public string? Notes { get; set; }
}
