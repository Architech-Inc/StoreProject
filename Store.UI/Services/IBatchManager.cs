using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;

namespace StoreUI.Services;

public interface IBatchManager
{
    Task<BatchMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<BatchDto>> GetBatchesPagedAsync(BatchFilterRequest request, CancellationToken ct = default);
    Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30, CancellationToken ct = default);
    Task<BatchDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BatchDto> CreateAsync(CreateBatchRequest request, CancellationToken ct = default);
    Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> WriteOffAsync(WriteOffBatchRequest request, CancellationToken ct = default);
}
