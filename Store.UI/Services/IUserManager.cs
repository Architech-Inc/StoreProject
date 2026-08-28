using Microsoft.AspNetCore.Http;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace StoreUI.Services;

public interface IUserManager
{
    Task<PagedResult<UserDto>> GetUsersPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<int> GetPendingContactChangesCountAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User360Dto?> Get360ByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, IFormFile? image, int? cropX, int? cropY, int? cropW, int? cropH, CancellationToken ct = default);
    Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request, IFormFile? image, int? cropX, int? cropY, int? cropW, int? cropH, CancellationToken ct = default);
    Task<UserDto?> SuspendUserAsync(Guid userId, CancellationToken ct = default);
    Task<string?> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default);
}
