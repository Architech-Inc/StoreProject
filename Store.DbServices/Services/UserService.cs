using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Users;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        return user is null ? null : MapToDto(user);
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<User>().Query()
            .Include(u => u.Role)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(u => u.Username.Contains(request.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Username)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => MapToDto(u))
            .ToListAsync(ct);

        return new PagedResult<UserDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (await _uow.Repository<User>().ExistsAsync(u => u.Username == request.Username.Trim(), ct))
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

        await _uow.Repository<User>().AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(user.UserId, ct))!;
    }

    public async Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Username) && request.Username.Trim() != user.Username)
        {
            if (await _uow.Repository<User>().ExistsAsync(u => u.Username == request.Username.Trim() && u.UserId != userId, ct))
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
            user.Username = request.Username.Trim();
        }

        if (request.RoleId.HasValue) user.RoleId = request.RoleId.Value;
        if (request.Status.HasValue) user.Status = request.Status.Value;

        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(userId, ct);
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user is null) return false;

        // Soft delete
        user.Status = UserStatus.Deleted;
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var userPassword = await _uow.Repository<UserPassword>().Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (userPassword is null) return false;

        if (!BCrypt.Net.BCrypt.EnhancedVerify(request.CurrentPassword, userPassword.PasswordHash))
            return false;

        userPassword.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword, 12);
        _uow.Repository<UserPassword>().Update(userPassword);
        await _uow.SaveChangesAsync(ct);
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
