using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;

namespace Store.Models.Interfaces.Services;

public interface IItemService
{
    Task<ItemDto?> GetByIdAsync(Guid itemId, CancellationToken ct = default);
    Task<PagedResult<ItemDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<IEnumerable<ItemDto>> GetLowStockAsync(CancellationToken ct = default);
    Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken ct = default);
    Task<ItemDto?> UpdateAsync(Guid itemId, UpdateItemRequest request, CancellationToken ct = default);
    Task<bool> AdjustStockAsync(Guid itemId, AdjustStockRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid itemId, CancellationToken ct = default);
}
