using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Discount : BaseEntity
{
    public int DiscountId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    /// <summary>Percentage off (0-100). Used when DiscountType = Percentage.</summary>
    public decimal Percentage { get; set; }

    /// <summary>Fixed amount off. Used when DiscountType = FixedAmount.</summary>
    public decimal? FixedAmount { get; set; }

    /// <summary>Null = all items.</summary>
    public Guid? ItemId { get; set; }

    /// <summary>Null = all categories. Ignored when ItemId is set.</summary>
    public int? CategoryId { get; set; }

    /// <summary>Minimum quantity required to trigger this discount.</summary>
    public int MinQuantity { get; set; } = 1;

    /// <summary>Null = all segments.</summary>
    public CustomerSegment? TargetSegment { get; set; }

    /// <summary>Optional coupon code. Null = auto-applied.</summary>
    [MaxLength(50)]
    public string? CouponCode { get; set; }

    /// <summary>Null = unlimited uses.</summary>
    public int? MaxUses { get; set; }

    public int UsedCount { get; set; } = 0;

    public Guid? ManagedByUserId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Item? Item { get; set; }
    public Category? Category { get; set; }
    public User? ManagedByUser { get; set; }
}
