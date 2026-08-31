using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderMetricsDto> GetPurchaseOrderMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseOrderFilterRequest request, CancellationToken ct = default);
    Task<List<PurchaseOrderDto>> GetAllAsync(PurchaseOrderStatus? status = null, Guid? supplierId = null);
    Task<PurchaseOrderDto?> GetByIdAsync(int id);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, Guid requestedByUserId);
    Task<PurchaseOrderDto?> SubmitAsync(int id, Guid userId);
    Task<PurchaseOrderDto?> ApproveAsync(int id, Guid approvedByUserId);
    Task<PurchaseOrderDto?> ReceiveAsync(int id, ReceivePurchaseOrderRequest request, Guid receivedByUserId);
    Task<PurchaseOrderDto?> CancelAsync(int id, Guid userId);
    Task<AutomatedReorderResultDto> ExecuteAutomatedReorderAsync(Guid? actingUserId = null, CancellationToken ct = default);
}
