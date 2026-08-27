using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiEmployeeService : IEmployeeService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiEmployeeService> _logger;

    public ApiEmployeeService(IApiClientService client, ILogger<ApiEmployeeService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _client.GetAsync<EmployeeDto>($"/api/employees/{employeeId}", ct);
    }

    public async Task<Employee360Dto?> Get360ByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _client.GetAsync<Employee360Dto>($"/api/employees/{employeeId}/360", ct);
    }

    public async Task<PagedResult<EmployeeDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}&includeInactive={request.IncludeInactive.ToString().ToLower()}";
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            qs += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";
        }
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            qs += $"&sortBy={Uri.EscapeDataString(request.SortBy)}";
        }
        if (request is EmployeeFilterRequest filterReq)
        {
            if (filterReq.DepartmentId.HasValue)
            {
                qs += $"&departmentId={filterReq.DepartmentId.Value}";
            }
            if (!string.IsNullOrWhiteSpace(filterReq.Status))
            {
                qs += $"&status={Uri.EscapeDataString(filterReq.Status)}";
            }
        }
        var result = await _client.GetAsync<PagedResult<EmployeeDto>>($"/api/employees{qs}", ct);
        return result ?? new PagedResult<EmployeeDto>();
    }

    public async Task<EmployeeMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var result = await _client.GetAsync<EmployeeMetricsDto>("/api/employees/metrics", ct);
        return result ?? new EmployeeMetricsDto();
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<EmployeeDto>("/api/employees", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create employee");
    }

    public async Task<EmployeeDto?> UpdateAsync(Guid employeeId, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        return await _client.PutAsync<EmployeeDto>($"/api/employees/{employeeId}", request, ct);
    }

    public async Task<bool> DeleteAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync($"/api/employees/{employeeId}", ct);
    }
}
