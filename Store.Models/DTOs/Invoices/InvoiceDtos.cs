using System.ComponentModel.DataAnnotations;
using Store.Models.DTOs.Common;
using Store.Models.Enums;

namespace Store.Models.DTOs.Invoices;

public class InvoiceDto
{
    public Guid InvoiceId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerSegment { get; set; }
    public Guid? UserId { get; set; }
    public string? ProcessedBy { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeGiven { get; set; }
    public decimal OutstandingBalance { get; set; }
    public PaymentType PaymentType { get; set; }
    public string TenderSummary { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public bool IsRefunded { get; set; }
    public decimal RefundedAmount { get; set; }
    public int LinesCount { get; set; }
    public string? Notes { get; set; }
    public DateTime DateCreated { get; set; }
    public IEnumerable<SaleLineDto> Lines { get; set; } = Enumerable.Empty<SaleLineDto>();
    public IEnumerable<InvoiceTenderDto> Tenders { get; set; } = Enumerable.Empty<InvoiceTenderDto>();
}

public class SaleLineDto
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? UnitAbbreviation { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class InvoicePagedRequest : PagedRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; } // "all" | "paid" | "unpaid" | "voided" | "refunded"
    public PaymentType? PaymentType { get; set; }
    public int? BranchId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

public class InvoiceSummaryMetricsDto
{
    public decimal GrossSales { get; set; }
    public decimal GrossRevenue { get => GrossSales; set => GrossSales = value; }

    public decimal CollectedRevenue { get; set; }
    public decimal TotalCollected { get => CollectedRevenue; set => CollectedRevenue = value; }

    public decimal OutstandingReceivables { get; set; }
    public decimal OutstandingBalance { get => OutstandingReceivables; set => OutstandingReceivables = value; }

    public decimal RefundedVolume { get; set; }
    public decimal TotalRefunded { get => RefundedVolume; set => RefundedVolume = value; }

    public decimal VoidedVolume { get; set; }

    public int TotalInvoicesCount { get; set; }
    public int TotalCount { get => TotalInvoicesCount; set => TotalInvoicesCount = value; }

    public int PaidCount { get; set; }
    public int UnpaidCount { get; set; }
    public int RefundedCount { get; set; }
    public int VoidedCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class CreateInvoiceRequest
{
    public Guid? CustomerId { get; set; }

    [Required]
    public PaymentType PaymentType { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal AmountTendered { get; set; }

    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public IEnumerable<CreateSaleLineRequest> Lines { get; set; } = Enumerable.Empty<CreateSaleLineRequest>();

    public string? CouponCode { get; set; }
}

public class CreateSaleLineRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal? OverrideUnitPrice { get; set; }
}

public class AddTenderRequest
{
    [Required]
    public PaymentType PaymentType { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public string? Reference { get; set; }
}

public class InvoiceTenderDto
{
    public int InvoiceTenderId { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime DateCreated { get; set; }
}

public class RefundInvoiceRequest
{
    [Required, MinLength(1)]
    public IEnumerable<RefundLineRequest> Lines { get; set; } = Enumerable.Empty<RefundLineRequest>();
    
    [Required]
    public string ReasonCode { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public bool RestockItems { get; set; } = true;
}

public class RefundLineRequest
{
    [Required]
    public Guid ItemId { get; set; }
    
    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

