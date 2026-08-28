using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class RoleMatrixModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<RoleMatrixDto> Matrix { get; private set; } = Array.Empty<RoleMatrixDto>();
    public IReadOnlyList<string> PermissionColumns { get; } = PermissionKeys.All;

    // ─── KPI Metrics ──────────────────────────────────────────────────────────
    public int TotalRoles => Matrix.Count;
    public int TotalCapabilities => PermissionColumns.Count;
    public int ElevatedRolesCount => Matrix.Count(r => r.Permissions.Any(p => p.Key.StartsWith("admin.", StringComparison.OrdinalIgnoreCase) && p.Value));
    public int GrantedCapabilitiesCount => Matrix.Sum(r => r.Permissions.Count(p => p.Value));
    public double SecurityGrantRatio => (TotalRoles * TotalCapabilities) > 0
        ? Math.Round((double)GrantedCapabilitiesCount / (TotalRoles * TotalCapabilities) * 100, 1)
        : 0;

    // ─── Permission Domains ───────────────────────────────────────────────────
    public static readonly IReadOnlyList<PermissionDomainGroup> DomainGroups = new List<PermissionDomainGroup>
    {
        new("Inventory & Operations", "domain-inventory", "#0284c7", new[] { PermissionKeys.InventoryRead, PermissionKeys.InventoryWrite }),
        new("Pricing & Margins", "domain-pricing", "#7c3aed", new[] { PermissionKeys.PricingRead, PermissionKeys.PricingWrite }),
        new("Cash & Settlement", "domain-cash", "#059669", new[] { PermissionKeys.CashRead, PermissionKeys.CashWrite, PermissionKeys.PaymentsRead, PermissionKeys.ReportsRead }),
        new("Administration & Security", "domain-admin", "#dc2626", new[] { PermissionKeys.AdminBranches, PermissionKeys.AdminRoleMatrix })
    };

    public IReadOnlyList<PermissionDomainGroup> Domains => DomainGroups;

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
            return AccessDenied();
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
            return AccessDenied();
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

    public async Task<IActionResult> OnPostToggleAjaxAsync([FromBody] UpdateRolePermissionRequest request, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return new JsonResult(new { success = false, message = "Authentication required." }) { StatusCode = 401 };
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.AdminRoleMatrix))
        {
            return StatusCode(403, new { success = false, message = "Access denied: AdminRoleMatrix permission required." });
        }

        if (request == null || string.IsNullOrWhiteSpace(request.PermissionKey))
        {
            return BadRequest(new { success = false, message = "Invalid permission request." });
        }

        try
        {
            var result = await _apiClient.PostAsync<RolePermissionDto>("/api/admin/role-matrix/permission", request, ct);
            if (result is null)
            {
                return StatusCode(500, new { success = false, message = "Failed to update role permission on server." });
            }

            return new JsonResult(new
            {
                success = true,
                roleId = result.RoleId,
                roleName = result.RoleName,
                permissionKey = result.PermissionKey,
                isAllowed = result.IsAllowed,
                message = $"{result.RoleName}: '{result.PermissionKey}' is now {(result.IsAllowed ? "Allowed" : "Denied")}."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public record PermissionDomainGroup(string DomainName, string ColorClass, string BadgeColor, IReadOnlyList<string> Permissions);

