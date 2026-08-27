using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.DTOs.Items;

namespace StoreUI.Services;

public interface IWastageManager
{
    Task<WastageMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<WastageEntryDto>> GetWastagePagedAsync(WastageFilterRequest request, CancellationToken ct = default);
    Task<WastageEntryDto> RecordWastageAsync(RecordWastageRequest request, Guid recordedByUserId, CancellationToken ct = default);
    Task<bool> DeleteWastageAsync(int id, CancellationToken ct = default);
    Task<List<ItemDto>> SearchCatalogItemsAsync(string? query, CancellationToken ct = default);
    byte[] ExportCsv(IEnumerable<WastageEntryDto> entries);
}
