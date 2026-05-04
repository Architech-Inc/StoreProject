using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Inter-branch stock transfer request (EX-FR-2.2 / MCT-3).
/// Lifecycle: Requested → Approved → Dispatched → Received (or Cancelled at any step).
/// </summary>
public class StockTransfer : BaseEntity
{
    [Key]
    public int StockTransferId { get; set; }

    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }

    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public Guid? DispatchedByUserId { get; set; }
    public Guid? ReceivedByUserId { get; set; }

    public StockTransferStatus Status { get; set; } = StockTransferStatus.Requested;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Navigation
    public Branch FromBranch { get; set; } = null!;
    public Branch ToBranch { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
