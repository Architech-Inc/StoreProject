using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Discounts;

public class DiscountOverrideDto
{
    public int DiscountOverrideRequestId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string OverrideType { get; set; } = string.Empty;
    public decimal OverrideValue { get; set; }
    public string? Justification { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RequestedByUser { get; set; } = string.Empty;
    public string? ReviewedByUser { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime DateCreated { get; set; }
}

public class CreateDiscountOverrideRequest
{
    public Guid? InvoiceId { get; set; }
    public Guid? ItemId { get; set; }

    public DiscountType OverrideType { get; set; } = DiscountType.Percentage;

    [Range(0.01, double.MaxValue, ErrorMessage = "Override value must be positive.")]
    public decimal OverrideValue { get; set; }

    [StringLength(1000)]
    public string? Justification { get; set; }
}

public class ReviewDiscountOverrideRequest
{
    [Required]
    public bool Approved { get; set; }

    [StringLength(1000)]
    public string? ReviewNotes { get; set; }
}
