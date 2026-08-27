using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Items;
using Store.Models.Entities;

namespace StoreUI.Services;

public interface IDiscountManager
{
    Task<DiscountMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<DiscountDto>> GetDiscountsPagedAsync(DiscountFilterRequest request, CancellationToken ct = default);
    Task<DiscountDto?> GetDiscountByIdAsync(int id, CancellationToken ct = default);
    Task<DiscountDto> CreateDiscountAsync(CreateDiscountRequest request, Guid userId, CancellationToken ct = default);
    Task<DiscountDto?> UpdateDiscountAsync(int id, UpdateDiscountRequest request, CancellationToken ct = default);
    Task<bool> DeleteDiscountAsync(int id, CancellationToken ct = default);
    Task<DiscountSimulationResult> SimulateDiscountAsync(DiscountSimulationRequest request, CancellationToken ct = default);
    Task<List<ItemDto>> SearchCatalogItemsAsync(string? query, CancellationToken ct = default);
    Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default);
    byte[] ExportCsv(IEnumerable<DiscountDto> discounts);
}
