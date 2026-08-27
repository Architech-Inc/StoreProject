using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Procurement;

namespace StoreUI.Services;

public interface IPurchaseOrderManager
{
    Task<PurchaseOrderMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseOrderFilterRequest request, CancellationToken ct = default);
    Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(int id, CancellationToken ct = default);
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, Guid userId, CancellationToken ct = default);
    Task<PurchaseOrderDto?> SubmitPurchaseOrderAsync(int id, Guid userId, CancellationToken ct = default);
    Task<PurchaseOrderDto?> ApprovePurchaseOrderAsync(int id, Guid userId, CancellationToken ct = default);
    Task<PurchaseOrderDto?> ReceivePurchaseOrderAsync(int id, ReceivePurchaseOrderRequest request, Guid userId, CancellationToken ct = default);
    Task<PurchaseOrderDto?> CancelPurchaseOrderAsync(int id, Guid userId, CancellationToken ct = default);
    Task<List<SupplierDto>> SearchSuppliersAsync(string? query, CancellationToken ct = default);
    Task<List<BranchDto>> SearchBranchesAsync(string? query, CancellationToken ct = default);
    Task<List<ItemDto>> SearchCatalogItemsAsync(string? query, CancellationToken ct = default);
    byte[] ExportCsv(IEnumerable<PurchaseOrderDto> orders);
}
