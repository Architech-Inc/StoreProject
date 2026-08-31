using Store.Models.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace StoreUI.Services;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IUserService _userService;
    private readonly IEmployeeService _employeeService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<CurrentUserContext> _logger;

    private UserProfileHeaderDto? _cachedProfile;

    public CurrentUserContext(
        IUserService userService,
        IEmployeeService employeeService,
        IApiClientService apiClient,
        ILogger<CurrentUserContext> logger)
    {
        _userService = userService;
        _employeeService = employeeService;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<UserProfileHeaderDto> GetCurrentProfileAsync(string? token, CancellationToken ct = default)
    {
        if (_cachedProfile != null)
        {
            return _cachedProfile;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _cachedProfile = new UserProfileHeaderDto(
                DisplayName: "Guest",
                AvatarUrl: "/images/admin.png",
                RoleName: "Guest",
                EmployeeInfo: string.Empty,
                UserId: null,
                Permissions: new HashSet<string>()
            );
            return _cachedProfile;
        }

        var perms = JwtPermissionReader.GetPermissions(token);
        var username = JwtPermissionReader.GetClaim(token, "sub") ?? "User";
        var userRole = JwtPermissionReader.GetClaim(token, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") ?? "Staff";
        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");

        string displayName = username;
        string avatarUrl = "/images/admin.png";
        string employeeInfo = string.Empty;
        Guid? parsedUserId = null;

        _apiClient.SetToken(token);

        if (Guid.TryParse(userIdStr, out var userId))
        {
            parsedUserId = userId;
            try
            {
                var currentUser = await _userService.GetByIdAsync(userId, ct);
                if (currentUser != null)
                {
                    avatarUrl = currentUser.ThumbnailUrl ?? currentUser.FullImageUrl ?? "/images/admin.png";
                    if (currentUser.EmployeeId.HasValue)
                    {
                        var emp = await _employeeService.GetByIdAsync(currentUser.EmployeeId.Value, ct);
                        if (emp != null)
                        {
                            if (!string.IsNullOrWhiteSpace(emp.FirstName))
                            {
                                displayName = emp.FirstName;
                            }
                            employeeInfo = $"Emp: {emp.DepartmentName ?? "Unassigned"}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve full user/employee profile for header context for user {UserId}", userId);
            }
        }

        _cachedProfile = new UserProfileHeaderDto(
            DisplayName: displayName,
            AvatarUrl: avatarUrl,
            RoleName: userRole,
            EmployeeInfo: employeeInfo,
            UserId: parsedUserId,
            Permissions: perms
        );

        return _cachedProfile;
    }
}
