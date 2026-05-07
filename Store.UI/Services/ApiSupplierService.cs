using Store.Models.DTOs.Procurement;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiSupplierService : ISupplierService
{
    private readonly IApiClientService _client;

    public ApiSupplierService(IApiClientService client) => _client = client;

    public async Task<List<SupplierDto>> GetAllAsync()
        => await _client.GetAsync<List<SupplierDto>>("/api/suppliers") ?? new();

    public async Task<SupplierDto?> GetByIdAsync(Guid id)
        => await _client.GetAsync<SupplierDto>($"/api/suppliers/{id}");

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
