using OtpNet;
using Store.Models.DTOs.Users;
using Store.Models.DTOs.Employees;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.DbServices.Context;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;
using Store.Models.Interfaces.Repositories.Users;
using Store.Models.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Store.DbServices.Services;

public class UserService : IUserService
{
    private readonly IUserAggregateRepository _users;
    private readonly StoreDbContext _db;

    public UserService(IUserAggregateRepository users, StoreDbContext db)
    {
        _users = users;
        _db = db;
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

    public async Task<User360Dto?> Get360ByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await GetByIdAsync(userId, ct);
        if (profile == null) return null;

        var dto = new User360Dto
        {
            Profile = profile,
            RecentActivity = await GetRecentActivityAsync(userId, ct),
            PendingContactChanges = await GetPendingContactChangesByUserIdAsync(userId, ct),
            ActiveSessions = await GetActiveSessionsAsync(userId, ct)
        };

        if (profile.EmployeeId.HasValue)
        {
            dto.LinkedEmployee = await _db.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeId == profile.EmployeeId.Value)
                .Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    FirstName = e.FirstName,
                    MiddleName = e.MiddleName,
                    LastName = e.LastName,
                    Gender = e.Gender,
                    DateEmployed = e.DateEmployed,
                    Status = e.Status,
                    DateCreated = e.DateCreated
                })
                .FirstOrDefaultAsync(ct);
        }

        return dto;
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

    public async Task<bool> Disable2FAAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdForUpdateAsync(userId, ct);
        if (user == null) return false;

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        _users.UpdateUser(user);

        await _users.AddAuditLogAsync(new AuditLog
        {
            UserId = userId,
            Action = "Security",
            Details = "Disabled Two-Factor Authentication"
        }, ct);

        await _users.SaveChangesAsync(ct);
        return true;
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

    public async Task<IReadOnlyCollection<Store.Models.DTOs.Auth.UserSessionDto>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var sessions = await _db.UserTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && !t.IsRevoked && t.RefreshTokenExpiryDate > DateTime.UtcNow)
            .OrderByDescending(t => t.LastActive)
            .Select(t => new Store.Models.DTOs.Auth.UserSessionDto
            {
                SessionId = t.UserTokenId,
                IpAddress = t.IpAddress,
                UserAgent = t.UserAgent,
                DeviceName = t.DeviceName,
                DateCreated = t.DateCreated,
                LastActive = t.LastActive,
                IsRevoked = t.IsRevoked
            })
            .ToListAsync(ct);
            
        return sessions;
    }

    public Task<bool> RevokeAllSessionsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException("Handled by AuthenticationService directly on the API side.");
    }

    public Task<bool> RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        throw new NotImplementedException("Handled by AuthenticationService directly on the API side.");
    }

    public async Task<ContactChangeRequestDto> RequestContactChangeAsync(Guid userId, CreateContactChangeDto request, CancellationToken ct = default)
    {
        var existingRequest = await _db.ContactChangeRequests
            .FirstOrDefaultAsync(r => r.UserId == userId && (r.Status == ContactChangeStatus.PendingVerification || r.Status == ContactChangeStatus.PendingApproval), ct);

        if (existingRequest != null)
        {
            throw new InvalidOperationException("You already have a pending contact change request.");
        }

        var changeRequest = new ContactChangeRequest
        {
            UserId = userId,
            NewEmail = request.NewEmail,
            NewPhone = request.NewPhone,
            VerificationToken = Guid.NewGuid().ToString("N"),
            Status = ContactChangeStatus.PendingVerification
        };

        _db.ContactChangeRequests.Add(changeRequest);
        await _db.SaveChangesAsync(ct);

        return new ContactChangeRequestDto
        {
            Id = changeRequest.Id,
            UserId = changeRequest.UserId,
            NewEmail = changeRequest.NewEmail,
            NewPhone = changeRequest.NewPhone,
            Status = changeRequest.Status,
            DateCreated = changeRequest.DateCreated
        };
    }

    public async Task<bool> VerifyContactChangeAsync(string token, CancellationToken ct = default)
    {
        var request = await _db.ContactChangeRequests
            .FirstOrDefaultAsync(r => r.VerificationToken == token && r.Status == ContactChangeStatus.PendingVerification, ct);

        if (request == null) return false;

        request.Status = ContactChangeStatus.PendingApproval;
        request.VerifiedAt = DateTime.UtcNow;
        request.VerificationToken = null; // Token consumed

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyCollection<ContactChangeRequestDto>> GetPendingContactChangesAsync(CancellationToken ct = default)
    {
        var requests = await _db.ContactChangeRequests
            .Include(r => r.User)
            .Where(r => r.Status == ContactChangeStatus.PendingApproval || r.Status == ContactChangeStatus.PendingVerification)
            .OrderByDescending(r => r.DateCreated)
            .Select(r => new ContactChangeRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                NewEmail = r.NewEmail,
                NewPhone = r.NewPhone,
                Status = r.Status,
                DateCreated = r.DateCreated,
                VerifiedAt = r.VerifiedAt
            })
            .ToListAsync(ct);

        return requests;
    }

    public async Task<IReadOnlyCollection<ContactChangeRequestDto>> GetPendingContactChangesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var requests = await _db.ContactChangeRequests
            .Include(r => r.User)
            .Where(r => r.UserId == userId && (r.Status == ContactChangeStatus.PendingApproval || r.Status == ContactChangeStatus.PendingVerification))
            .OrderByDescending(r => r.DateCreated)
            .Select(r => new ContactChangeRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                NewEmail = r.NewEmail,
                NewPhone = r.NewPhone,
                Status = r.Status,
                DateCreated = r.DateCreated,
                VerifiedAt = r.VerifiedAt
            })
            .ToListAsync(ct);

        return requests;
    }

    public async Task<bool> ApproveContactChangeAsync(Guid requestId, Guid approvedById, CancellationToken ct = default)
    {
        var request = await _db.ContactChangeRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == ContactChangeStatus.PendingApproval, ct);

        if (request == null) return false;

        request.Status = ContactChangeStatus.Approved;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovedById = approvedById;

        // Apply the actual changes to the user
        // We will just update primary email/phone for now by updating UserContacts through existing service
        var updateContactsRequest = new UpdateUserContactsRequest
        {
            Email = request.NewEmail,
            Phone = request.NewPhone
        };
        
        await UpdateContactsAsync(request.UserId, updateContactsRequest, ct);
        
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectContactChangeAsync(Guid requestId, Guid rejectedById, CancellationToken ct = default)
    {
        var request = await _db.ContactChangeRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && (r.Status == ContactChangeStatus.PendingApproval || r.Status == ContactChangeStatus.PendingVerification), ct);

        if (request == null) return false;

        request.Status = ContactChangeStatus.Rejected;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovedById = rejectedById; // Store who rejected it in the same field

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelContactChangeAsync(Guid requestId, Guid userId, CancellationToken ct = default)
    {
        var request = await _db.ContactChangeRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId && 
                (r.Status == ContactChangeStatus.PendingApproval || r.Status == ContactChangeStatus.PendingVerification), ct);

        if (request == null) return false;

        request.Status = ContactChangeStatus.Cancelled;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyCollection<ContactChangeRequestDto>> GetContactChangeHistoryAsync(CancellationToken ct = default)
    {
        var requests = await _db.ContactChangeRequests
            .Include(r => r.User)
            .Where(r => r.Status == ContactChangeStatus.Approved || r.Status == ContactChangeStatus.Rejected || r.Status == ContactChangeStatus.Cancelled)
            .OrderByDescending(r => r.DateCreated)
            .Select(r => new ContactChangeRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                NewEmail = r.NewEmail,
                NewPhone = r.NewPhone,
                Status = r.Status,
                DateCreated = r.DateCreated,
                VerifiedAt = r.VerifiedAt
            })
            .ToListAsync(ct);

        return requests;
    }
}
