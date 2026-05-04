using Store.Models.DTOs.Inventory;

namespace Store.Models.Interfaces.Services;

public interface IBatchService
{
    Task<List<BatchDto>> GetAllAsync(Guid? itemId = null, string? expiryStatus = null);
    Task<BatchDto?> GetByIdAsync(Guid id);
    Task<BatchDto> CreateAsync(CreateBatchRequest request);
    Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request);
    Task<bool> DeleteAsync(Guid id);

    /// <summary>Returns batches expiring within the given number of days.</summary>
    Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30);
}
