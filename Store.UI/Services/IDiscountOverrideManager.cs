using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Invoices;
using Store.Models.DTOs.Items;

namespace StoreUI.Services;

public interface IDiscountOverrideManager
{
    Task<DiscountOverrideMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<DiscountOverrideDto>> GetOverridesPagedAsync(DiscountOverrideFilterRequest request, CancellationToken ct = default);
    Task<DiscountOverrideDto?> GetOverrideByIdAsync(int id, CancellationToken ct = default);
    Task<DiscountOverrideDto> CreateOverrideAsync(CreateDiscountOverrideRequest request, Guid requestedByUserId, CancellationToken ct = default);
    Task<DiscountOverrideDto?> ReviewOverrideAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request, CancellationToken ct = default);
    Task<bool> CancelOverrideAsync(int id, Guid userId, CancellationToken ct = default);
    Task<List<InvoiceDto>> SearchInvoicesAsync(string? query, CancellationToken ct = default);
    Task<List<ItemDto>> SearchItemsAsync(string? query, CancellationToken ct = default);
    byte[] ExportCsv(IEnumerable<DiscountOverrideDto> overrides);
}
