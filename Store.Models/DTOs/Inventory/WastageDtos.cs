using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Inventory;

public class WastageEntryDto
{
    public int WastageEntryId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string WastageType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceCode { get; set; }
    public string RecordedByUser { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
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
