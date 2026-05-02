using Store.Models.DTOs.Common;
using Store.Models.Entities;

namespace Store.Models.Interfaces.Repositories.Users;

public interface IUserAggregateRepository
{
    Task<User?> GetByIdWithRoleEmployeeAsync(Guid userId, bool asNoTracking, CancellationToken ct = default);
    Task<(IReadOnlyCollection<User> Users, int Total)> GetPagedUsersWithRoleAsync(PagedRequest request, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<User?> GetByIdForUpdateAsync(Guid userId, CancellationToken ct = default);
    Task AddUserAsync(User user, CancellationToken ct = default);
    void UpdateUser(User user);
    Task<UserPassword?> GetUserPasswordAsync(Guid userId, CancellationToken ct = default);
    void UpdateUserPassword(UserPassword userPassword);
    Task SaveChangesAsync(CancellationToken ct = default);
}
