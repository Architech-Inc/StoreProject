using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace Store.Models.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid userId, CancellationToken ct = default);
}
