using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Cash;

public class CashVarianceMetricsDto
{
    public int TotalRecords { get; set; }
    public int TotalPendingCount { get; set; }
    public int TotalReviewedCount { get; set; }
    public int TotalEscalatedCount { get; set; }
    public decimal NetDiscrepancyXaf { get; set; }
    public decimal TotalShortagesXaf { get; set; }
    public decimal TotalOveragesXaf { get; set; }
}

public class CashVarianceDto
{
    public int CashVarianceRecordId { get; set; }
    public Guid CashierShiftId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => ActualAmount - ExpectedAmount;
    public bool IsShortage => Variance < 0;
    public bool IsOverage => Variance > 0;
    public bool IsExactMatch => Variance == 0;
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RecordedByUserId { get; set; }
    public string RecordedByUser { get; set; } = string.Empty;
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByUser { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime DateCreated { get; set; }

    public DateTime? ShiftOpenedAtUtc { get; set; }
    public DateTime? ShiftClosedAtUtc { get; set; }
    public decimal? ShiftOpeningFloat { get; set; }
}

public class RecordCashVarianceRequest
{
    [Required]
    public Guid CashierShiftId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ExpectedAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ActualAmount { get; set; }

    [StringLength(100)]
    public string? ReasonCode { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public class ReviewCashVarianceRequest
{
    [Required]
    public CashVarianceStatus Status { get; set; }

    [StringLength(2000)]
    public string? ReviewNotes { get; set; }
}

public static class CashVarianceReasonCodes
{
    public const string CountingError = "COUNTING_ERROR";
    public const string TillFloatShort = "TILL_FLOAT_SHORT";
    public const string UnloggedChangeDrawer = "UNLOGGED_CHANGE_DRAWER";
    public const string CounterfeitDetected = "COUNTERFEIT_DETECTED";
    public const string TheftSuspected = "THEFT_SUSPECTED";
    public const string SystemGlitch = "SYSTEM_GLITCH";
    public const string Other = "OTHER";

    public static readonly IReadOnlyList<string> All = new[]
    {
        CountingError,
        TillFloatShort,
        UnloggedChangeDrawer,
        CounterfeitDetected,
        TheftSuspected,
        SystemGlitch,
        Other
    };
}
