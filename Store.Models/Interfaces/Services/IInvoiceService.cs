using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;

namespace Store.Models.Interfaces.Services;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default);
    Task<PagedResult<InvoiceDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default);
    Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, CancellationToken ct = default);
}
