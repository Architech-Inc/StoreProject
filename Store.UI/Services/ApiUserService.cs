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
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            qs += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";
        }
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

    public async Task<UserDto?> UpdateAvatarAsync(string? thumbUrl, string? fullUrl, CancellationToken ct = default)
    {
        var request = new UpdateUserRequest
        {
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl
        };
        return await _client.PutAsync<UserDto>("/api/users/profile/avatar", request, ct);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<bool?>($"/api/users/change-password", request, ct);
        return result.HasValue && result.Value;
    }

    public async Task<bool> UpdateContactsAsync(Guid userId, UpdateUserContactsRequest request, CancellationToken ct = default)
    {
        var result = await _client.PutAsync<object>("/api/users/profile/contacts", request, ct);
        return result != null;
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await _client.DeleteAsync($"/api/users/{userId}", ct);
        return result;
    }

    public async Task<string?> GetAvatarByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        var result = await _client.GetAsync<string>($"/api/auth/avatar/{Uri.EscapeDataString(username.Trim())}", ct);
        return result;
    }

    public async Task<string?> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        try 
        {
            return await _client.PostAsync<string>($"/api/users/{userId}/issue-temp-password", null, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Enable2FAResponse> Enable2FAAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<Enable2FAResponse>("/api/users/profile/2fa/enable", null, ct);
        return result ?? throw new InvalidOperationException("Failed to initiate 2FA.");
    }

    public async Task<bool> Verify2FAAsync(Guid userId, Verify2FARequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<object>("/api/users/profile/2fa/verify", request, ct);
        return result != null;
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetRecentActivityAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await _client.GetAsync<IReadOnlyCollection<AuditLogDto>>("/api/users/profile/activity", ct);
        return result ?? Array.Empty<AuditLogDto>();
    }

    public async Task<bool> RevokeAllSessionsAsync(CancellationToken ct = default)
    {
        var result = await _client.PostAsync<object>("/api/users/profile/sessions/revoke", null, ct);
        return result != null;
    }
}
