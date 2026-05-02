using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiCustomerService : ICustomerService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiCustomerService> _logger;

    public ApiCustomerService(IApiClientService client, ILogger<ApiCustomerService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _client.GetAsync<CustomerDto>($"/api/customers/{customerId}", ct);
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        var result = await _client.GetAsync<PagedResult<CustomerDto>>($"/api/customers{qs}", ct);
        return result ?? new PagedResult<CustomerDto>();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<CustomerDto>("/api/customers", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create customer");
    }

    public async Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        return await _client.PutAsync<CustomerDto>($"/api/customers/{customerId}", request, ct);
    }

    public async Task<bool> DeleteAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync($"/api/customers/{customerId}", ct);
    }
}
