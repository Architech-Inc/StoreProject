using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Records a wastage/shrinkage stock write-off event (INV-3).
/// Each entry decrements stock for the affected item.
/// </summary>
public class WastageEntry : BaseEntity
{
    [Key]
    public int WastageEntryId { get; set; }

    public Guid ItemId { get; set; }

    public WastageType WastageType { get; set; }

    /// <summary>Number of units written off (always positive).</summary>
    public int Quantity { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Optional reference (e.g. inspection report number).</summary>
    [MaxLength(100)]
    public string? ReferenceCode { get; set; }

    /// <summary>User who logged this wastage event.</summary>
    public Guid RecordedByUserId { get; set; }

    // Navigation
    public Item Item { get; set; } = null!;
    public User RecordedByUser { get; set; } = null!;
}
