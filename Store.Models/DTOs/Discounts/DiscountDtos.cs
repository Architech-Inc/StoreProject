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
    /// <summary>Set to true to remove item targeting and apply to all items.</summary>
    public bool ClearItemId { get; set; }

    public int? CategoryId { get; set; }
    /// <summary>Set to true to remove category targeting and apply to all categories.</summary>
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
