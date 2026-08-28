using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;

namespace StoreUI.Services;

public interface IInvoiceManager
{
    Task<PagedResult<InvoiceDto>> GetInvoicesPagedAsync(InvoicePagedRequest request, CancellationToken ct = default);
    Task<InvoiceSummaryMetricsDto> GetSummaryMetricsAsync(InvoicePagedRequest request, CancellationToken ct = default);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default);
    Task<bool> VoidInvoiceAsync(Guid invoiceId, string? reason, CancellationToken ct = default);
    Task<InvoiceTenderDto> AddTenderAsync(Guid invoiceId, AddTenderRequest request, CancellationToken ct = default);
    Task<InvoiceDto?> RefundInvoiceAsync(Guid invoiceId, RefundInvoiceRequest request, CancellationToken ct = default);
    byte[] GenerateInvoicesCsv(IEnumerable<InvoiceDto> items);
}
