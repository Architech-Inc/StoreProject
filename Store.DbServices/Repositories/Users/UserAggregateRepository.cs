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
        => _context.UserPasswords.Update(userPassword);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
