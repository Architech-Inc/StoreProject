using Store.API.Application.Users.Ports;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;

namespace Store.API.Infrastructure.Users;

public class UsersPort : IUsersPort
{
    private readonly IUserService _userService;

    public UsersPort(IUserService userService)
    {
        _userService = userService;
    }

    public Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
        => _userService.GetAllAsync(request, ct);

    public Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        => _userService.GetByIdAsync(userId, ct);

    public Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        => _userService.CreateAsync(request, ct);

    public Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
        => _userService.UpdateAsync(userId, request, ct);

    public Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default)
        => _userService.DeleteAsync(userId, ct);

    public Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
        => _userService.ChangePasswordAsync(userId, request, ct);
}
