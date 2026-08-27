using Store.Models.DTOs.Common;
using Store.Models.DTOs.Transfers;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class StockTransferManager : IStockTransferManager
{
    private readonly IStockTransferService _transferService;

    public StockTransferManager(IStockTransferService transferService)
    {
        _transferService = transferService;
    }

    public async Task<TransferMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _transferService.GetTransferMetricsAsync(ct);

    public async Task<PagedResult<StockTransferDto>> GetTransfersPagedAsync(TransferFilterRequest request, CancellationToken ct = default)
        => await _transferService.GetTransfersPagedAsync(request, ct);

    public async Task<List<StockTransferDto>> GetAllAsync(int? branchId = null, string? status = null, CancellationToken ct = default)
        => await _transferService.GetAllAsync(branchId, status);

    public async Task<StockTransferDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _transferService.GetByIdAsync(id);

    public async Task<StockTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedByUserId, CancellationToken ct = default)
        => await _transferService.CreateAsync(request, requestedByUserId);

    public async Task<StockTransferDto?> ApproveAsync(int id, Guid approvedByUserId, ApproveTransferRequest request, CancellationToken ct = default)
        => await _transferService.ApproveAsync(id, approvedByUserId, request);

    public async Task<bool> RejectAsync(int id, Guid userId, RejectTransferRequest request, CancellationToken ct = default)
        => await _transferService.RejectAsync(id, userId, request);

    public async Task<StockTransferDto?> DispatchAsync(int id, Guid dispatchedByUserId, DispatchTransferRequest request, CancellationToken ct = default)
        => await _transferService.DispatchAsync(id, dispatchedByUserId, request);

    public async Task<StockTransferDto?> ReceiveAsync(int id, Guid receivedByUserId, ReceiveTransferRequest request, CancellationToken ct = default)
        => await _transferService.ReceiveAsync(id, receivedByUserId, request);

    public async Task<bool> CancelAsync(int id, Guid userId, string? reason, CancellationToken ct = default)
        => await _transferService.CancelAsync(id, userId, reason);
}
