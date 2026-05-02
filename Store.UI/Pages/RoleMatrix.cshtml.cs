using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class RoleMatrixModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<RoleMatrixDto> Matrix { get; private set; } = Array.Empty<RoleMatrixDto>();
    public IReadOnlyList<string> PermissionColumns { get; } = PermissionKeys.All;

    [TempData] public string? StatusMessage { get; set; }

    public RoleMatrixModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.AdminRoleMatrix))
        {
            return Forbid();
        }

        Matrix = await _apiClient.GetAsync<List<RoleMatrixDto>>("/api/admin/role-matrix", ct)
            ?? new List<RoleMatrixDto>();

        return Page();
    }

    public async Task<IActionResult> OnPostToggleAsync(int roleId, string permissionKey, bool currentValue, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.AdminRoleMatrix))
        {
            return Forbid();
        }

        var req = new UpdateRolePermissionRequest
        {
            RoleId = roleId,
            PermissionKey = permissionKey,
            IsAllowed = !currentValue
        };

        var result = await _apiClient.PostAsync<RolePermissionDto>("/api/admin/role-matrix/permission", req, ct);
        StatusMessage = result is null
            ? "Permission update failed."
            : $"{result.RoleName}: {result.PermissionKey} = {(result.IsAllowed ? "Allowed" : "Denied")}";

        return RedirectToPage();
    }
}
