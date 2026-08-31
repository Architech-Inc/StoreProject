namespace StoreUI.Services;

public record UserProfileHeaderDto(
    string DisplayName,
    string AvatarUrl,
    string RoleName,
    string EmployeeInfo,
    Guid? UserId,
    IReadOnlySet<string> Permissions
);

public interface ICurrentUserContext
{
    Task<UserProfileHeaderDto> GetCurrentProfileAsync(string? token, CancellationToken ct = default);
}
