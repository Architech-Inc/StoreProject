using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace Store.Models.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User360Dto?> Get360ByIdAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateAvatarAsync(string? thumbUrl, string? fullUrl, CancellationToken ct = default);
    Task<string?> GetAvatarByUsernameAsync(string username, CancellationToken ct = default);
    Task<string?> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UpdateContactsAsync(Guid userId, UpdateUserContactsRequest request, CancellationToken ct = default);
    
    // 2FA & Security
    Task<Enable2FAResponse> Enable2FAAsync(Guid userId, CancellationToken ct = default);
    Task<bool> Verify2FAAsync(Guid userId, Verify2FARequest request, CancellationToken ct = default);
    Task<bool> Disable2FAAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<AuditLogDto>> GetRecentActivityAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Store.Models.DTOs.Auth.UserSessionDto>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> RevokeAllSessionsAsync(CancellationToken ct = default);
    Task<bool> RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default);

    // Contact Change Workflow
    Task<ContactChangeRequestDto> RequestContactChangeAsync(Guid userId, CreateContactChangeDto request, CancellationToken ct = default);
    Task<bool> VerifyContactChangeAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyCollection<ContactChangeRequestDto>> GetPendingContactChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<ContactChangeRequestDto>> GetPendingContactChangesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ApproveContactChangeAsync(Guid requestId, Guid approvedById, CancellationToken ct = default);
    Task<bool> RejectContactChangeAsync(Guid requestId, Guid rejectedById, CancellationToken ct = default);
    Task<bool> CancelContactChangeAsync(Guid requestId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<ContactChangeRequestDto>> GetContactChangeHistoryAsync(CancellationToken ct = default);
}
