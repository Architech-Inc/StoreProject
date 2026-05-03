using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Operations;

public class TaxProfileDto
{
    public int TaxProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal RatePercent { get; set; }
    public TaxApplicationType ApplicationType { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertTaxProfileRequest
{
    public int? TaxProfileId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal RatePercent { get; set; }

    public TaxApplicationType ApplicationType { get; set; } = TaxApplicationType.Exclusive;
    public bool IsActive { get; set; } = true;
}

public class BundleRuleDto
{
    public int BundleRuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TriggerItemId { get; set; }
    public string TriggerItemName { get; set; } = string.Empty;
    public Guid RewardItemId { get; set; }
    public string RewardItemName { get; set; } = string.Empty;
    public int TriggerQuantity { get; set; }
    public int RewardQuantity { get; set; }
    public decimal RewardDiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertBundleRuleRequest
{
    public int? BundleRuleId { get; set; }

    [Required, StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid TriggerItemId { get; set; }

    [Required]
    public Guid RewardItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int TriggerQuantity { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int RewardQuantity { get; set; } = 1;

    [Range(0, 100)]
    public decimal RewardDiscountPercent { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SegmentPricingDto
{
    public long CustomerSegmentPriceId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public CustomerSegment Segment { get; set; }
    public decimal PriceOverride { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertSegmentPricingRequest
{
    public long? CustomerSegmentPriceId { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;

    [Range(0, double.MaxValue)]
    public decimal PriceOverride { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PricingPreviewRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
}

public class PricingPreviewDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal BaseUnitPrice { get; set; }
    public decimal TaxRatePercent { get; set; }
    public decimal SegmentPrice { get; set; }
    public decimal DiscountPerUnit { get; set; }
    public decimal BundleDiscountTotal { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
}

// ─── Promotion Effectiveness ──────────────────────────────────────────────────

public class PromotionEffectivenessDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public int InvoicesWithDiscount { get; set; }
    public List<ItemDiscountSummaryDto> TopDiscountedItems { get; set; } = new();
    public List<BundleHitSummaryDto> BundleHits { get; set; } = new();
    public List<SegmentEffectivenessSummaryDto> SegmentSummary { get; set; } = new();
}

public class ItemDiscountSummaryDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal DiscountPercent { get; set; }
    public int UnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscountGiven { get; set; }
}

public class BundleHitSummaryDto
{
    public int BundleRuleId { get; set; }
    public string BundleName { get; set; } = string.Empty;
    public string TriggerItemName { get; set; } = string.Empty;
    public string RewardItemName { get; set; } = string.Empty;
    public decimal RewardDiscountPercent { get; set; }
    public int TriggerInvoiceCount { get; set; }
}

public class SegmentEffectivenessSummaryDto
{
    public string Segment { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal SegmentPrice { get; set; }
    public int UnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}
