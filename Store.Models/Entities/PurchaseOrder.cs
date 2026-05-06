using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>Purchase order header — tracks procurement intent from creation to receipt (EX-FR-1.2).</summary>
public class PurchaseOrder : BaseEntity
{
    [Key]
    public int PurchaseOrderId { get; set; }

    /// <summary>Human-readable reference number (e.g. PO-2024-001).</summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    public Guid SupplierId { get; set; }

    public int? BranchId { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public DateTime? ExpectedDeliveryDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public Guid RequestedByUserId { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public Branch? Branch { get; set; }
    public User RequestedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
