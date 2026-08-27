using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;

namespace Store.Models.Interfaces.Services;

public interface IBatchService
{
    Task<List<BatchDto>> GetAllAsync(Guid? itemId = null, string? expiryStatus = null);
    Task<PagedResult<BatchDto>> GetBatchesPagedAsync(BatchFilterRequest request, CancellationToken ct = default);
    Task<BatchMetricsDto> GetBatchMetricsAsync(CancellationToken ct = default);
    Task<BatchDto?> GetByIdAsync(Guid id);
    Task<BatchDto> CreateAsync(CreateBatchRequest request);
    Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> WriteOffBatchAsync(WriteOffBatchRequest request, Guid? actingUserId, CancellationToken ct = default);

    /// <summary>Returns batches expiring within the given number of days.</summary>
    Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30);
}
