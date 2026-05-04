using Store.Models.DTOs.Discounts;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiDiscountService : IDiscountService
{
    private readonly IApiClientService _client;

    public ApiDiscountService(IApiClientService client) => _client = client;

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
}
