using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiUserService : IUserService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiUserService> _logger;

    public ApiUserService(IApiClientService client, ILogger<ApiUserService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _client.GetAsync<UserDto>($"/api/users/{userId}", ct);
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        var result = await _client.GetAsync<PagedResult<UserDto>>($"/api/users{qs}", ct);
        return result ?? new PagedResult<UserDto>();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<UserDto>("/api/users", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create user");
    }

    public async Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        return await _client.PutAsync<UserDto>($"/api/users/{userId}", request, ct);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<bool?>($"/api/users/{userId}/change-password", request, ct);
        return result.HasValue && result.Value;
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync($"/api/users/{userId}", ct);
    }
}
