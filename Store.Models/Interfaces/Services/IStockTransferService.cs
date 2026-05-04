using Store.Models.DTOs.Transfers;

namespace Store.Models.Interfaces.Services;

public interface IStockTransferService
{
    Task<List<StockTransferDto>> GetAllAsync(int? branchId = null, string? status = null);
    Task<StockTransferDto?> GetByIdAsync(int id);
    Task<StockTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedByUserId);
    Task<StockTransferDto?> ApproveAsync(int id, Guid approvedByUserId, ApproveTransferRequest request);
    Task<bool> RejectAsync(int id, Guid userId, RejectTransferRequest request);
    Task<StockTransferDto?> DispatchAsync(int id, Guid dispatchedByUserId, DispatchTransferRequest request);
    Task<StockTransferDto?> ReceiveAsync(int id, Guid receivedByUserId, ReceiveTransferRequest request);
    Task<bool> CancelAsync(int id, Guid userId, string? reason);
}
