using Store.Models.DTOs.Users;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Repositories.Users;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class UserService : IUserService
{
    private readonly IUserAggregateRepository _users;

    public UserService(IUserAggregateRepository users)
    {
        _users = users;
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdWithRoleEmployeeAsync(userId, asNoTracking: true, ct);

        return user is null ? null : MapToDto(user);
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var (users, total) = await _users.GetPagedUsersWithRoleAsync(request, ct);
        var items = users.Select(MapToDto).ToList();

        return new PagedResult<UserDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (await _users.UsernameExistsAsync(request.Username, ct: ct))
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username.Trim(),
            RoleId = request.RoleId,
            Status = UserStatus.NotVerified
        };

        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, 12);
        user.Password = new UserPassword
        {
            UserId = user.UserId,
            PasswordHash = passwordHash
        };

        await _users.AddUserAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return (await GetByIdAsync(user.UserId, ct))!;
    }

    public async Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdForUpdateAsync(userId, ct);

        if (user is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Username) && request.Username.Trim() != user.Username)
        {
            if (await _users.UsernameExistsAsync(request.Username, userId, ct))
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
            user.Username = request.Username.Trim();
        }

        if (request.RoleId.HasValue) user.RoleId = request.RoleId.Value;
        if (request.Status.HasValue) user.Status = request.Status.Value;

        _users.UpdateUser(user);
        await _users.SaveChangesAsync(ct);

        return await GetByIdAsync(userId, ct);
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdForUpdateAsync(userId, ct);

        if (user is null) return false;

        // Soft delete
        user.Status = UserStatus.Deleted;
        _users.UpdateUser(user);
        await _users.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var userPassword = await _users.GetUserPasswordAsync(userId, ct);

        if (userPassword is null) return false;

        if (!BCrypt.Net.BCrypt.EnhancedVerify(request.CurrentPassword, userPassword.PasswordHash))
            return false;

        userPassword.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword, 12);
        _users.UpdateUserPassword(userPassword);
        await _users.SaveChangesAsync(ct);
        return true;
    }

    private static UserDto MapToDto(User u) => new()
    {
        UserId = u.UserId,
        Username = u.Username,
        RoleId = u.RoleId,
        RoleName = u.Role?.Name,
        EmployeeId = u.EmployeeId,
        Status = u.Status,
        ImagePath = u.ImagePath,
        DateCreated = u.DateCreated
    };
}
