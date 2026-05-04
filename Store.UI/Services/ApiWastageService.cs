using Store.Models.DTOs.Inventory;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiWastageService : IWastageService
{
    private readonly IApiClientService _client;

    public ApiWastageService(IApiClientService client) => _client = client;

    public async Task<List<WastageEntryDto>> GetAllAsync(Guid? itemId = null, string? wastageType = null)
    {
        var qs = new List<string>();
        if (itemId.HasValue) qs.Add($"itemId={itemId.Value}");
        if (!string.IsNullOrWhiteSpace(wastageType)) qs.Add($"wastageType={Uri.EscapeDataString(wastageType)}");
        var query = qs.Count > 0 ? "?" + string.Join("&", qs) : "";
        return await _client.GetAsync<List<WastageEntryDto>>($"/api/wastage{query}") ?? new();
    }

    public async Task<WastageEntryDto?> GetByIdAsync(int id)
        => await _client.GetAsync<WastageEntryDto>($"/api/wastage/{id}");

    public async Task<WastageEntryDto> RecordAsync(RecordWastageRequest request, Guid recordedByUserId)
    {
        var result = await _client.PostAsync<WastageEntryDto>("/api/wastage", request);
        return result ?? throw new InvalidOperationException("Failed to record wastage.");
    }

    public async Task<bool> DeleteAsync(int id)
        => await _client.DeleteAsync($"/api/wastage/{id}");
}
