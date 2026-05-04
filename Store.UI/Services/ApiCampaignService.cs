using Store.Models.DTOs.Loyalty;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiCampaignService : ILoyaltyCampaignService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiCampaignService> _logger;

    public ApiCampaignService(IApiClientService client, ILogger<ApiCampaignService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<LoyaltyCampaignDto>> GetAllAsync(bool? activeOnly, CancellationToken ct = default)
    {
        var qs = activeOnly.HasValue ? $"?activeOnly={activeOnly.Value}" : "";
        var result = await _client.GetAsync<IEnumerable<LoyaltyCampaignDto>>($"/api/loyaltycampaigns{qs}", ct);
        return result ?? Enumerable.Empty<LoyaltyCampaignDto>();
    }

    public async Task<LoyaltyCampaignDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _client.GetAsync<LoyaltyCampaignDto>($"/api/loyaltycampaigns/{id}", ct);
    }

    public async Task<LoyaltyCampaignDto> CreateAsync(CreateCampaignRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<LoyaltyCampaignDto>("/api/loyaltycampaigns", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create campaign.");
    }

    public async Task<LoyaltyCampaignDto?> UpdateAsync(int id, UpdateCampaignRequest request, CancellationToken ct = default)
    {
        return await _client.PutAsync<LoyaltyCampaignDto>($"/api/loyaltycampaigns/{id}", request, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _client.DeleteAsync($"/api/loyaltycampaigns/{id}", ct);
    }

    public async Task<IEnumerable<LoyaltyCampaignDto>> GetActiveCampaignsForSegmentAsync(string segment, CancellationToken ct = default)
    {
        var result = await _client.GetAsync<IEnumerable<LoyaltyCampaignDto>>($"/api/loyaltycampaigns/active?segment={Uri.EscapeDataString(segment)}", ct);
        return result ?? Enumerable.Empty<LoyaltyCampaignDto>();
    }
}
