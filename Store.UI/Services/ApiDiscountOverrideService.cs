using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiDiscountOverrideService : IDiscountOverrideService
{
    private readonly IApiClientService _client;

    public ApiDiscountOverrideService(IApiClientService client) => _client = client;

    public async Task<DiscountOverrideMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _client.GetAsync<DiscountOverrideMetricsDto>("/api/discount-overrides/metrics") ?? new();

    public async Task<PagedResult<DiscountOverrideDto>> GetOverridesPagedAsync(DiscountOverrideFilterRequest request, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            qs.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        if (!string.IsNullOrWhiteSpace(request.Status))
            qs.Add($"status={Uri.EscapeDataString(request.Status)}");

        if (!string.IsNullOrWhiteSpace(request.OverrideType))
            qs.Add($"overrideType={Uri.EscapeDataString(request.OverrideType)}");

        var url = $"/api/discount-overrides/paged?{string.Join("&", qs)}";
        return await _client.GetAsync<PagedResult<DiscountOverrideDto>>(url) ?? new();
    }

    public async Task<List<DiscountOverrideDto>> GetAllAsync(string? status = null)
    {
        var query = string.IsNullOrWhiteSpace(status) ? "" : $"?status={Uri.EscapeDataString(status)}";
        return await _client.GetAsync<List<DiscountOverrideDto>>($"/api/discount-overrides{query}") ?? new();
    }

    public async Task<DiscountOverrideDto?> GetByIdAsync(int id)
        => await _client.GetAsync<DiscountOverrideDto>($"/api/discount-overrides/{id}");

    public async Task<DiscountOverrideDto> CreateAsync(CreateDiscountOverrideRequest request, Guid requestedByUserId)
    {
        var result = await _client.PostAsync<DiscountOverrideDto>("/api/discount-overrides", request);
        return result ?? throw new InvalidOperationException("Failed to create discount override request.");
    }

    public async Task<DiscountOverrideDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request)
        => await _client.PostAsync<DiscountOverrideDto>($"/api/discount-overrides/{id}/review", request);

    public async Task<bool> CancelAsync(int id, Guid userId)
        => await _client.PostAsync($"/api/discount-overrides/{id}/cancel", null);
}
