using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces.Repositories.Users;

namespace Store.DbServices.Repositories.Users;

public class UserAggregateRepository : IUserAggregateRepository
{
    private readonly StoreDbContext _context;

    public UserAggregateRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdWithRoleEmployeeAsync(Guid userId, bool asNoTracking, CancellationToken ct = default)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .Where(u => u.UserId == userId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyCollection<User> Users, int Total)> GetPagedUsersWithRoleAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => u.Username.Contains(request.SearchTerm));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Username)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        var normalized = username.Trim();

        return _context.Users.AnyAsync(
            u => u.Username == normalized && (!excludeUserId.HasValue || u.UserId != excludeUserId.Value),
            ct);
    }

    public Task<User?> GetByIdForUpdateAsync(Guid userId, CancellationToken ct = default)
        => _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);

    public async Task AddUserAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public void UpdateUser(User user)
        => _context.Users.Update(user);

    public Task<UserPassword?> GetUserPasswordAsync(Guid userId, CancellationToken ct = default)
        => _context.UserPasswords.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public void UpdateUserPassword(UserPassword userPassword)
    {
        _context.UserPasswords.Update(userPassword);
    }

    public async Task<string?> GetAvatarByUsernameAsync(string username, CancellationToken ct = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Username.ToLower() == username.ToLower())
            .Select(u => new { u.ThumbnailUrl })
            .FirstOrDefaultAsync(ct);

        return user?.ThumbnailUrl;
    }
    public async Task<User?> GetUserWithContactsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Emails).ThenInclude(ue => ue.Email)
            .Include(u => u.Phones).ThenInclude(up => up.Phone)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

    public async Task UpdateUserContactsAsync(User user, string? email, string? phone, CancellationToken ct = default)
    {
        // Handle Email
        var currentPrimaryEmail = user.Emails.FirstOrDefault(e => e.IsPrimary);
        if (email != null)
        {
            if (currentPrimaryEmail == null)
            {
                user.Emails.Add(new Store.Models.Entities.Contacts.UserEmail
                {
                    IsPrimary = true,
                    Email = new Store.Models.Entities.Contacts.Email { Address = email.Trim(), Type = Store.Models.Enums.EmailType.Personal }
                });
            }
            else if (currentPrimaryEmail.Email.Address != email.Trim())
            {
                currentPrimaryEmail.Email.Address = email.Trim();
            }
        }
        else if (currentPrimaryEmail != null)
        {
            _context.UserEmails.Remove(currentPrimaryEmail);
            _context.Emails.Remove(currentPrimaryEmail.Email);
        }

        // Handle Phone
        var currentPrimaryPhone = user.Phones.FirstOrDefault(p => p.IsPrimary);
        if (phone != null)
        {
            if (currentPrimaryPhone == null)
            {
                var defaultCountry = await _context.Countries.FirstOrDefaultAsync(ct);
                var countryId = defaultCountry?.CountryId ?? 1; // Fallback
                
                user.Phones.Add(new Store.Models.Entities.Contacts.UserPhone
                {
                    IsPrimary = true,
                    Phone = new Store.Models.Entities.Contacts.Phone { Number = phone.Trim(), CountryId = countryId, Type = Store.Models.Enums.PhoneType.Mobile }
                });
            }
            else if (currentPrimaryPhone.Phone.Number != phone.Trim())
            {
                currentPrimaryPhone.Phone.Number = phone.Trim();
            }
        }
        else if (currentPrimaryPhone != null)
        {
            _context.UserPhones.Remove(currentPrimaryPhone);
            _context.Phones.Remove(currentPrimaryPhone.Phone);
        }

        _context.Users.Update(user);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task AddAuditLogAsync(AuditLog log, CancellationToken ct = default)
    {
        await _context.AuditLogs.AddAsync(log, ct);
    }

    public async Task<IReadOnlyCollection<AuditLog>> GetRecentActivityAsync(Guid userId, int limit = 10, CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateCreated)
            .Take(limit)
            .ToListAsync(ct);
    }
}
