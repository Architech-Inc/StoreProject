using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Records a cash discrepancy detected at shift close and tracks manager review (EX-FR-5.2).
/// </summary>
public class CashVarianceRecord : BaseEntity
{
    [Key]
    public int CashVarianceRecordId { get; set; }

    public Guid CashierShiftId { get; set; }

    /// <summary>Expected (system-calculated) cash total at close.</summary>
    public decimal ExpectedAmount { get; set; }

    /// <summary>Actual counted cash amount reported by cashier.</summary>
    public decimal ActualAmount { get; set; }

    /// <summary>Short reason code, e.g. "COUNTING_ERROR", "THEFT", "VOID_DISCREPANCY".</summary>
    [MaxLength(100)]
    public string? ReasonCode { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public CashVarianceStatus Status { get; set; } = CashVarianceStatus.Pending;

    public Guid RecordedByUserId { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    [MaxLength(2000)]
    public string? ReviewNotes { get; set; }

    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public CashierShift CashierShift { get; set; } = null!;
    public User RecordedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
