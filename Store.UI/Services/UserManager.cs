using Microsoft.AspNetCore.Http;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class UserManager : IUserManager
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;

    public UserManager(IUserService userService, IFileService fileService)
    {
        _userService = userService;
        _fileService = fileService;
    }

    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        return await _userService.GetAllAsync(request, ct);
    }

    public async Task<int> GetPendingContactChangesCountAsync(CancellationToken ct = default)
    {
        var pendingChanges = await _userService.GetPendingContactChangesAsync(ct);
        return pendingChanges.Count(p => p.Status == ContactChangeStatus.PendingApproval);
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _userService.GetByIdAsync(userId, ct);
    }

    public async Task<User360Dto?> Get360ByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _userService.Get360ByIdAsync(userId, ct);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, IFormFile? image, int? cropX, int? cropY, int? cropW, int? cropH, CancellationToken ct = default)
    {
        if (image != null && image.Length > 0)
        {
            using var stream = image.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, image.FileName, image.ContentType, "users", cropX, cropY, cropW, cropH, ct);
            request.ThumbnailUrl = uploadResult.ThumbnailUrl;
            request.FullImageUrl = uploadResult.FullImageUrl;
        }

        return await _userService.CreateAsync(request, ct);
    }

    public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request, IFormFile? image, int? cropX, int? cropY, int? cropW, int? cropH, CancellationToken ct = default)
    {
        if (image != null && image.Length > 0)
        {
            var existingUser = await _userService.GetByIdAsync(userId, ct);
            if (existingUser != null)
            {
                if (!string.IsNullOrWhiteSpace(existingUser.ThumbnailUrl))
                    await _fileService.DeleteFileAsync(existingUser.ThumbnailUrl, ct);
                if (!string.IsNullOrWhiteSpace(existingUser.FullImageUrl))
                    await _fileService.DeleteFileAsync(existingUser.FullImageUrl, ct);
            }

            using var stream = image.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, image.FileName, image.ContentType, "users", cropX, cropY, cropW, cropH, ct);
            request.ThumbnailUrl = uploadResult.ThumbnailUrl;
            request.FullImageUrl = uploadResult.FullImageUrl;
        }

        return await _userService.UpdateAsync(userId, request, ct);
    }

    public async Task<UserDto?> SuspendUserAsync(Guid userId, CancellationToken ct = default)
    {
        var update = new UpdateUserRequest { Status = UserStatus.Suspended };
        return await _userService.UpdateAsync(userId, update, ct);
    }

    public async Task<string?> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        return await _userService.IssueTempPasswordAsync(userId, ct);
    }

    public async Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        await _userService.RevokeAllSessionsAsync(userId, ct);
    }
}
