using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Cash;

public class CashVarianceDto
{
    public int CashVarianceRecordId { get; set; }
    public Guid CashierShiftId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => ActualAmount - ExpectedAmount;
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RecordedByUser { get; set; } = string.Empty;
    public string? ReviewedByUser { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime DateCreated { get; set; }
}

public class RecordCashVarianceRequest
{
    [Required]
    public Guid CashierShiftId { get; set; }

    public decimal ExpectedAmount { get; set; }

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
