using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Invoices;

public class InvoiceDto
{
    public Guid InvoiceId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? UserId { get; set; }
    public string? ProcessedBy { get; set; }
    public int? BranchId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeGiven { get; set; }
    public decimal OutstandingBalance { get; set; }
    public PaymentType PaymentType { get; set; }
    public bool IsPaid { get; set; }
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
