using Store.API.Application.Users.Ports;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;

namespace Store.API.Infrastructure.Users;

public class UsersPort : IUsersPort
{
    private readonly IUserService _userService;
    private readonly IPasswordRecoveryService _passwordRecoveryService;

    public UsersPort(IUserService userService, IPasswordRecoveryService passwordRecoveryService)
    {
        _userService = userService;
        _passwordRecoveryService = passwordRecoveryService;
    }

    public Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
        => _userService.GetAllAsync(request, ct);

    public Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        => _userService.GetByIdAsync(userId, ct);

    public Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        => _userService.CreateAsync(request, ct);

    public Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
        => _userService.UpdateAsync(userId, request, ct);

    public Task<bool> UpdateContactsAsync(Guid userId, UpdateUserContactsRequest request, CancellationToken ct = default)
        => _userService.UpdateContactsAsync(userId, request, ct);

    public Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default)
        => _userService.DeleteAsync(userId, ct);

    public Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
        => _userService.ChangePasswordAsync(userId, request, ct);

    public Task<string?> GetAvatarAsync(string username, CancellationToken ct = default)
        => _userService.GetAvatarByUsernameAsync(username, ct);

    public Task<string> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default)
        => _passwordRecoveryService.IssueTempPasswordAsync(userId, ct);

    public Task<Enable2FAResponse> Enable2FAAsync(Guid userId, CancellationToken ct = default)
        => _userService.Enable2FAAsync(userId, ct);

    public Task<bool> Verify2FAAsync(Guid userId, Verify2FARequest request, CancellationToken ct = default)
        => _userService.Verify2FAAsync(userId, request, ct);

    public Task<IReadOnlyCollection<AuditLogDto>> GetRecentActivityAsync(Guid userId, CancellationToken ct = default)
        => _userService.GetRecentActivityAsync(userId, ct);
}
