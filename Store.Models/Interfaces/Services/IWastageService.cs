using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;

namespace Store.Models.Interfaces.Services;

public interface IWastageService
{
    Task<WastageMetricsDto> GetWastageMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<WastageEntryDto>> GetWastagePagedAsync(WastageFilterRequest request, CancellationToken ct = default);
    Task<List<WastageEntryDto>> GetAllAsync(Guid? itemId = null, string? wastageType = null);
    Task<WastageEntryDto?> GetByIdAsync(int id);
    Task<WastageEntryDto> RecordAsync(RecordWastageRequest request, Guid recordedByUserId);
    Task<bool> DeleteAsync(int id);
}
