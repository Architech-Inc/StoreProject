using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Discounts;

public class DiscountDto
{
    public int DiscountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public decimal? FixedAmount { get; set; }
    public Guid? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemBarcode { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int MinQuantity { get; set; }
    public string? TargetSegment { get; set; }
    public string? CouponCode { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrentlyValid { get; set; }
    public DateTime DateCreated { get; set; }

    public string ScopeType => ItemId.HasValue ? "Item" : CategoryId.HasValue ? "Category" : "StoreWide";
    public string ScopeLabel => ItemId.HasValue ? $"Product: {ItemName}" : CategoryId.HasValue ? $"Category: {CategoryName}" : "Store-Wide";
    public string ValueFormatted => DiscountType == "Percentage" ? $"{Percentage:0.##}% OFF" : $"{FixedAmount ?? 0:N0} XAF OFF";
}

public class DiscountMetricsDto
{
    public int TotalRules { get; set; }
    public int ActiveRulesCount { get; set; }
    public int CouponCampaignsCount { get; set; }
    public int SegmentRulesCount { get; set; }
    public int TotalRedemptionsCount { get; set; }
}

public class DiscountFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SearchTerm { get; set; }
    public string? DiscountType { get; set; }
    public string? TargetSegment { get; set; }
    public bool? ActiveOnly { get; set; }
    public bool? HasCoupon { get; set; }
    public string? Scope { get; set; } // Item, Category, StoreWide
}

public class DiscountSimulationRequest
{
    [Range(0, double.MaxValue)]
    public decimal ItemUnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public int? DiscountId { get; set; }
    public string? CouponCode { get; set; }
    public CustomerSegment? CustomerSegment { get; set; }
}

public class DiscountSimulationResult
{
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal OriginalUnitPrice { get; set; }
    public decimal OriginalTotalPrice { get; set; }
    public decimal TotalDiscountAmountXaf { get; set; }
    public decimal EffectiveUnitPrice { get; set; }
    public decimal FinalTotalPriceXaf { get; set; }
    public decimal SavingsPercentage { get; set; }
}

public class CreateDiscountRequest
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Range(0, 100)]
    public decimal Percentage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? FixedAmount { get; set; }

    public Guid? ItemId { get; set; }
    public int? CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int MinQuantity { get; set; } = 1;

    public CustomerSegment? TargetSegment { get; set; }

    [StringLength(50)]
    public string? CouponCode { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxUses { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDiscountRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    public DiscountType? DiscountType { get; set; }

    [Range(0, 100)]
    public decimal? Percentage { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? FixedAmount { get; set; }

    public Guid? ItemId { get; set; }
    public bool ClearItemId { get; set; }

    public int? CategoryId { get; set; }
    public bool ClearCategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int? MinQuantity { get; set; }

    public CustomerSegment? TargetSegment { get; set; }

    [StringLength(50)]
    public string? CouponCode { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxUses { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool? IsActive { get; set; }
}
