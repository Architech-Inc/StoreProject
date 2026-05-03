using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Operations;

public class ShiftOpenRequest
{
    [Range(0, double.MaxValue)]
    public decimal OpeningFloat { get; set; }

    public string? Notes { get; set; }
}

public class ShiftCloseRequest
{
    [Range(0, double.MaxValue)]
    public decimal ClosingFloat { get; set; }

    public string? Notes { get; set; }
}

public class CashierShiftDto
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
    public ShiftStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class PaymentBreakdownDto
{
    public PaymentType PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public int InvoiceCount { get; set; }
}

public class TopProductDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal GrossMargin { get; set; }
}

public class DailyZReportDto
{
    public DateTime Date { get; set; }
    public decimal GrossSales { get; set; }
    public decimal Discounts { get; set; }
    public decimal NetSales { get; set; }
    public decimal Cogs { get; set; }
    public decimal GrossMargin { get; set; }
    public int InvoiceCount { get; set; }
    public decimal AverageBasket { get; set; }
    public List<PaymentBreakdownDto> PaymentBreakdown { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class ShiftReconciliationDto
{
    public Guid CashierShiftId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingFloat { get; set; }
    public decimal? ExpectedClosingAmount { get; set; }
    public decimal? VarianceAmount { get; set; }
    public ShiftStatus Status { get; set; }
    public decimal CashSalesTotal { get; set; }
    public int InvoiceCount { get; set; }
    public IReadOnlyList<PaymentBreakdownDto> PaymentBreakdown { get; set; } = [];
}

public class DayEndReconciliationDto
{
    public DateOnly Date { get; set; }
    public int TotalShifts { get; set; }
    public int OpenShifts { get; set; }
    public decimal TotalCashSales { get; set; }
    public decimal TotalNonCashSales { get; set; }
    public decimal TotalVariance { get; set; }
    public IReadOnlyList<ShiftReconciliationDto> Shifts { get; set; } = [];
}
