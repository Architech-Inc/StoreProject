using OtpNet;
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
        if (user == null) return null;

        var userWithContacts = await _users.GetUserWithContactsAsync(userId, ct);
        if (userWithContacts != null)
        {
            user.Emails = userWithContacts.Emails;
            user.Phones = userWithContacts.Phones;
        }

        return MapToDto(user);
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
            Status = UserStatus.NotVerified,
            ThumbnailUrl = request.ThumbnailUrl?.Trim(),
            FullImageUrl = request.FullImageUrl?.Trim()
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
        if (request.ThumbnailUrl != null) user.ThumbnailUrl = request.ThumbnailUrl.Trim();
        if (request.FullImageUrl != null) user.FullImageUrl = request.FullImageUrl.Trim();

        _users.UpdateUser(user);
        await _users.SaveChangesAsync(ct);

        return await GetByIdAsync(userId, ct);
    }

    public Task<UserDto?> UpdateAvatarAsync(string? thumbUrl, string? fullUrl, CancellationToken ct = default)
    {
        // This is only called via the API controllers which use the mediator pattern and UpdateAsync.
        // It's implemented here just to satisfy the IUserService interface for any direct DI usage.
        throw new NotImplementedException("Use UpdateAsync instead for direct DB service calls.");
    }

    public async Task<bool> UpdateContactsAsync(Guid userId, UpdateUserContactsRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetUserWithContactsAsync(userId, ct);
        if (user == null) return false;

        await _users.UpdateUserContactsAsync(user, request.Email, request.Phone, ct);
        await _users.SaveChangesAsync(ct);
        return true;
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

    public async Task<string?> GetAvatarByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        return await _users.GetAvatarByUsernameAsync(username.Trim(), ct);
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
        TwoFactorEnabled = u.TwoFactorEnabled,
        ThumbnailUrl = u.ThumbnailUrl,
        FullImageUrl = u.FullImageUrl,
        DateCreated = u.DateCreated,
        PrimaryEmail = u.Emails?.FirstOrDefault(e => e.IsPrimary)?.Email?.Address,
        PrimaryPhone = u.Phones?.FirstOrDefault(p => p.IsPrimary)?.Phone?.Number
    };

    public Task<string?> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        // This is handled via MediatR and IUsersPort directly using IPasswordRecoveryService
        throw new NotImplementedException("Use IPasswordRecoveryService for this operation on the backend.");
    }

    public async Task<Enable2FAResponse> Enable2FAAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdForUpdateAsync(userId, ct);
        if (user == null) throw new InvalidOperationException("User not found.");

        var key = KeyGeneration.GenerateRandomKey(20);
        var base32Key = Base32Encoding.ToString(key);

        user.TwoFactorSecret = base32Key;
        // TwoFactorEnabled remains false until verified
        _users.UpdateUser(user);
        await _users.SaveChangesAsync(ct);

        var issuer = "Architech-Inc StoreProject";
        var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(user.Username)}?secret={base32Key}&issuer={Uri.EscapeDataString(issuer)}";

        return new Enable2FAResponse
        {
            SharedKey = base32Key,
            AuthenticatorUri = uri
        };
    }

    public async Task<bool> Verify2FAAsync(Guid userId, Verify2FARequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdForUpdateAsync(userId, ct);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret)) return false;

        var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
        var valid = totp.VerifyTotp(request.Code, out long timeStepMatched, window: new VerificationWindow(2, 2));

        if (valid)
        {
            user.TwoFactorEnabled = true;
            _users.UpdateUser(user);
            
            await _users.AddAuditLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "2FA Enabled",
                Details = "Two-factor authentication was successfully enabled."
            }, ct);
            
            await _users.SaveChangesAsync(ct);
            return true;
        }

        return false;
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetRecentActivityAsync(Guid userId, CancellationToken ct = default)
    {
        var logs = await _users.GetRecentActivityAsync(userId, 10, ct);
        return logs.Select(l => new AuditLogDto
        {
            Id = l.AuditLogId,
            Action = l.Action,
            Details = l.Details,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            DateCreated = l.DateCreated
        }).ToList();
    }

    public Task<bool> RevokeAllSessionsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException("Handled by AuthenticationService directly on the API side.");
    }
}
