using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace Store.API.Application.Users.Ports;

public interface IUsersPort
{
    Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default);
    Task<bool> UpdateContactsAsync(Guid userId, UpdateUserContactsRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<string?> GetAvatarAsync(string username, CancellationToken ct = default);
    Task<string> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default);
    
    // 2FA & Security
    Task<Enable2FAResponse> Enable2FAAsync(Guid userId, CancellationToken ct = default);
    Task<bool> Verify2FAAsync(Guid userId, Verify2FARequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<AuditLogDto>> GetRecentActivityAsync(Guid userId, CancellationToken ct = default);
}
