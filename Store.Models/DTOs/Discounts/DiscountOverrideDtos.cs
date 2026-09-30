using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Discounts;

public class DiscountOverrideDto
{
    public int DiscountOverrideRequestId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemBarcode { get; set; }
    public decimal? ItemUnitPrice { get; set; }
    public decimal? InvoiceTotalAmount { get; set; }
    public string OverrideType { get; set; } = string.Empty;
    public decimal OverrideValue { get; set; }
    public string? Justification { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public string RequestedByUser { get; set; } = string.Empty;
    public string? RequestedByFullName { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByUser { get; set; }
    public string? ReviewedByFullName { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime DateCreated { get; set; }

    // Wave 23.A — POS-side fields. Null for the legacy invoice-scoped
    // override flow (controller fills them when the request came from POS).
    public string? PosSessionId { get; set; }
    public string? CartFingerprint { get; set; }
    public DateTime? AppliedAt { get; set; }

    public string ScopeType => ItemId.HasValue ? "Item" : "Invoice";
    public string ScopeLabel => ItemId.HasValue
        ? $"Product: {ItemName}"
        : (InvoiceId.HasValue ? $"Invoice #{InvoiceId.Value.ToString()[..8]}" : "Cart-Level Override");

    public string ValueFormatted => OverrideType == "Percentage"
        ? $"{OverrideValue:0.##}% OFF"
        : $"{OverrideValue:N0} XAF OFF";

    public decimal EstimatedImpactXaf
    {
        get
        {
            if (OverrideType == "FixedAmount")
                return OverrideValue;

            if (ItemId.HasValue && ItemUnitPrice.HasValue)
                return Math.Round(ItemUnitPrice.Value * (OverrideValue / 100m), 2);

            if (InvoiceTotalAmount.HasValue)
                return Math.Round(InvoiceTotalAmount.Value * (OverrideValue / 100m), 2);

            return 0;
        }
    }
}

public class DiscountOverrideMetricsDto
{
    public int TotalRequests { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalEstimatedImpactXaf { get; set; }
}

public class DiscountOverrideFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? OverrideType { get; set; }
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

    // Wave 23.A — POS-side binding. Both fields are optional: when null,
    // the request is treated as the legacy invoice-scoped override. When
    // set, the server verifies PosSessionId + CartFingerprint on checkout.
    [StringLength(64)]
    public string? PosSessionId { get; set; }

    [StringLength(64)]
    public string? CartFingerprint { get; set; }
}

public class ReviewDiscountOverrideRequest
{
    [Required]
    public bool Approved { get; set; }

    [StringLength(1000)]
    public string? ReviewNotes { get; set; }
}
