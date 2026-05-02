using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class CashierShift : BaseEntity
{
    public Guid CashierShiftId { get; set; }
    public Guid OpenedByUserId { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingFloat { get; set; }
    public decimal? ExpectedClosingAmount { get; set; }
    public decimal? VarianceAmount { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public string? Notes { get; set; }

    public User OpenedByUser { get; set; } = null!;
    public User? ClosedByUser { get; set; }
}
