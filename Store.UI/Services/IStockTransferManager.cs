using Store.Models.DTOs.Common;
using Store.Models.DTOs.Transfers;

namespace StoreUI.Services;

public interface IStockTransferManager
{
    Task<TransferMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<StockTransferDto>> GetTransfersPagedAsync(TransferFilterRequest request, CancellationToken ct = default);
    Task<List<StockTransferDto>> GetAllAsync(int? branchId = null, string? status = null, CancellationToken ct = default);
    Task<StockTransferDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StockTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedByUserId, CancellationToken ct = default);
    Task<StockTransferDto?> ApproveAsync(int id, Guid approvedByUserId, ApproveTransferRequest request, CancellationToken ct = default);
    Task<bool> RejectAsync(int id, Guid userId, RejectTransferRequest request, CancellationToken ct = default);
    Task<StockTransferDto?> DispatchAsync(int id, Guid dispatchedByUserId, DispatchTransferRequest request, CancellationToken ct = default);
    Task<StockTransferDto?> ReceiveAsync(int id, Guid receivedByUserId, ReceiveTransferRequest request, CancellationToken ct = default);
    Task<bool> CancelAsync(int id, Guid userId, string? reason, CancellationToken ct = default);
}
