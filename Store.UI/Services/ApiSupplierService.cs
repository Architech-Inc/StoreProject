using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiSupplierService : ISupplierService
{
    private readonly IApiClientService _client;

    public ApiSupplierService(IApiClientService client) => _client = client;

    public async Task<List<SupplierDto>> GetAllAsync(string? search = null, string? city = null, string? country = null, string? sortBy = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(city)) queryParams.Add($"city={Uri.EscapeDataString(city)}");
        if (!string.IsNullOrWhiteSpace(country)) queryParams.Add($"country={Uri.EscapeDataString(country)}");
        if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        return await _client.GetAsync<List<SupplierDto>>($"/api/suppliers{queryString}") ?? new();
    }

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var queryParams = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (!string.IsNullOrWhiteSpace(request.SearchTerm)) queryParams.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(request.SortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

        var queryString = "?" + string.Join("&", queryParams);
        return await _client.GetAsync<PagedResult<SupplierDto>>($"/api/suppliers/paged{queryString}", ct) ?? new();
    }

    public async Task<SupplierDto?> GetByCodeOrNameAsync(string codeOrName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codeOrName)) return null;
        var suppliers = await GetAllAsync(search: codeOrName);
        var target = codeOrName.Trim().ToLower();
        return suppliers.FirstOrDefault(s =>
            (s.RegistrationNumber != null && s.RegistrationNumber.ToLower() == target) ||
            s.Name.ToLower() == target);
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id)
        => await _client.GetAsync<SupplierDto>($"/api/suppliers/{id}");

    public async Task<SupplierProfileDto?> GetProfileAsync(Guid id)
        => await _client.GetAsync<SupplierProfileDto>($"/api/suppliers/{id}/profile");

    public async Task<SupplierMetricsDto> GetMetricsAsync()
        => await _client.GetAsync<SupplierMetricsDto>("/api/suppliers/metrics") ?? new();

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, Guid createdByUserId)
    {
        var result = await _client.PostAsync<SupplierDto>("/api/suppliers", request);
        return result ?? throw new InvalidOperationException("Failed to create supplier.");
    }

    public async Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierRequest request)
        => await _client.PutAsync<SupplierDto>($"/api/suppliers/{id}", request);

    public async Task<bool> DeleteAsync(Guid id)
        => await _client.DeleteAsync($"/api/suppliers/{id}");
}
