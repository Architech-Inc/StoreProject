using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>
/// Tracks on-hand stock and branch-specific price overrides per physical store branch.
/// </summary>
public class BranchItemStock : BaseEntity
{
    public int BranchId { get; set; }
    public Guid ItemId { get; set; }

    /// <summary>On-hand inventory available for sale at this specific branch.</summary>
    public int InStock { get; set; }

    /// <summary>Branch-specific reorder alert threshold.</summary>
    public int? ReorderLevel { get; set; }

    /// <summary>
    /// Optional explicit unit price override for this branch.
    /// If null, falls back to Branch.PriceMultiplier * Item.UnitPrice (or Item.UnitPrice).
    /// </summary>
    public decimal? CustomUnitPrice { get; set; }

    /// <summary>Optional custom cost price for this branch.</summary>
    public decimal? CustomCostPrice { get; set; }

    // Navigation
    public Branch Branch { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
