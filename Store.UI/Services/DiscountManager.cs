using System.Text;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Items;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class DiscountManager : IDiscountManager
{
    private readonly IDiscountService _discountService;
    private readonly IItemService _itemService;
    private readonly ICategoryService _categoryService;

    public DiscountManager(
        IDiscountService discountService,
        IItemService itemService,
        ICategoryService categoryService)
    {
        _discountService = discountService;
        _itemService = itemService;
        _categoryService = categoryService;
    }

    public async Task<DiscountMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _discountService.GetMetricsAsync(ct);

    public async Task<PagedResult<DiscountDto>> GetDiscountsPagedAsync(DiscountFilterRequest request, CancellationToken ct = default)
        => await _discountService.GetDiscountsPagedAsync(request, ct);

    public async Task<DiscountDto?> GetDiscountByIdAsync(int id, CancellationToken ct = default)
        => await _discountService.GetByIdAsync(id);

    public async Task<DiscountDto> CreateDiscountAsync(CreateDiscountRequest request, Guid userId, CancellationToken ct = default)
        => await _discountService.CreateAsync(request, userId);

    public async Task<DiscountDto?> UpdateDiscountAsync(int id, UpdateDiscountRequest request, CancellationToken ct = default)
        => await _discountService.UpdateAsync(id, request);

    public async Task<bool> DeleteDiscountAsync(int id, CancellationToken ct = default)
        => await _discountService.DeleteAsync(id);

    public async Task<DiscountSimulationResult> SimulateDiscountAsync(DiscountSimulationRequest request, CancellationToken ct = default)
        => await _discountService.SimulateDiscountAsync(request, ct);

    public async Task<List<ItemDto>> SearchCatalogItemsAsync(string? query, CancellationToken ct = default)
    {
        var req = new PagedRequest
        {
            Page = 1,
            PageSize = 50,
            SearchTerm = query?.Trim()
        };
        var result = await _itemService.GetAllAsync(req, ct);
        return result.Items.ToList();
    }

    public async Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _categoryService.GetAllAsync(ct);
        return categories.OrderBy(c => c.Name).ToList();
    }

    public byte[] ExportCsv(IEnumerable<DiscountDto> discounts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Type,Value,Scope,Target Item/Category,Min Qty,Customer Segment,Coupon Code,Used Count,Max Uses,Valid From,Valid To,Active,Currently Valid");

        foreach (var d in discounts)
        {
            sb.AppendLine(string.Join(",",
                d.DiscountId,
                EscapeCsv(d.Name),
                EscapeCsv(d.DiscountType),
                EscapeCsv(d.ValueFormatted),
                EscapeCsv(d.ScopeType),
                EscapeCsv(d.ScopeLabel),
                d.MinQuantity,
                EscapeCsv(d.TargetSegment ?? "All"),
                EscapeCsv(d.CouponCode ?? "Auto"),
                d.UsedCount,
                d.MaxUses.HasValue ? d.MaxUses.Value.ToString() : "Unlimited",
                EscapeCsv(d.ValidFrom?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
                EscapeCsv(d.ValidTo?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
                d.IsActive,
                d.IsCurrentlyValid
            ));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return $"\"{field}\"";
    }
}
