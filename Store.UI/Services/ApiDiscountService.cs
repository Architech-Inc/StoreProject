using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiDiscountService : IDiscountService
{
    private readonly IApiClientService _client;

    public ApiDiscountService(IApiClientService client) => _client = client;

    public async Task<DiscountMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _client.GetAsync<DiscountMetricsDto>("/api/discounts/metrics") ?? new();

    public async Task<PagedResult<DiscountDto>> GetDiscountsPagedAsync(DiscountFilterRequest request, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            qs.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        if (!string.IsNullOrWhiteSpace(request.DiscountType))
            qs.Add($"discountType={Uri.EscapeDataString(request.DiscountType)}");

        if (!string.IsNullOrWhiteSpace(request.TargetSegment))
            qs.Add($"targetSegment={Uri.EscapeDataString(request.TargetSegment)}");

        if (request.ActiveOnly.HasValue)
            qs.Add($"activeOnly={request.ActiveOnly.Value}");

        if (request.HasCoupon.HasValue)
            qs.Add($"hasCoupon={request.HasCoupon.Value}");

        if (!string.IsNullOrWhiteSpace(request.Scope))
            qs.Add($"scope={Uri.EscapeDataString(request.Scope)}");

        var url = $"/api/discounts/paged?{string.Join("&", qs)}";
        return await _client.GetAsync<PagedResult<DiscountDto>>(url) ?? new();
    }

    public async Task<List<DiscountDto>> GetAllAsync(bool? activeOnly = null, string? couponCode = null)
    {
        var qs = new List<string>();
        if (activeOnly.HasValue) qs.Add($"activeOnly={activeOnly.Value}");
        if (!string.IsNullOrWhiteSpace(couponCode)) qs.Add($"couponCode={Uri.EscapeDataString(couponCode)}");
        var query = qs.Count > 0 ? "?" + string.Join("&", qs) : "";
        var result = await _client.GetAsync<List<DiscountDto>>($"/api/discounts{query}");
        return result ?? new List<DiscountDto>();
    }

    public async Task<DiscountDto?> GetByIdAsync(int id)
        => await _client.GetAsync<DiscountDto>($"/api/discounts/{id}");

    public async Task<DiscountDto> CreateAsync(CreateDiscountRequest request, Guid managedByUserId)
    {
        var result = await _client.PostAsync<DiscountDto>("/api/discounts", request);
        return result ?? throw new InvalidOperationException("Failed to create discount.");
    }

    public async Task<DiscountDto?> UpdateAsync(int id, UpdateDiscountRequest request)
        => await _client.PutAsync<DiscountDto>($"/api/discounts/{id}", request);

    public async Task<bool> DeleteAsync(int id)
        => await _client.DeleteAsync($"/api/discounts/{id}");

    public async Task<DiscountDto?> ValidateCouponAsync(string couponCode)
        => await _client.GetAsync<DiscountDto>($"/api/discounts/validate-coupon?code={Uri.EscapeDataString(couponCode)}");

    public async Task IncrementUsageAsync(int discountId)
        => await _client.PostAsync($"/api/discounts/{discountId}/increment-usage", null);

    public async Task<DiscountSimulationResult> SimulateDiscountAsync(DiscountSimulationRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<DiscountSimulationResult>("/api/discounts/simulate", request);
        return result ?? new DiscountSimulationResult();
    }
}
