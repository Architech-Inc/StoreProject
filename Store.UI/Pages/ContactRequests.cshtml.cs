using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Users;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ContactRequestsModel : SecurePageModel
{
    private readonly IContactRequestManager _contactManager;
    private readonly IApiClientService _apiClient;

    public IReadOnlyCollection<ContactChangeRequestDto> PendingContactChanges { get; private set; } = Array.Empty<ContactChangeRequestDto>();
    public IReadOnlyCollection<ContactChangeRequestDto> HistoricalContactChanges { get; private set; } = Array.Empty<ContactChangeRequestDto>();
    public ContactRequestMetricsDto Metrics { get; private set; } = new();

    [TempData] public string? StatusMessage { get; set; }

    public ContactRequestsModel(IContactRequestManager contactManager, IApiClientService apiClient)
    {
        _contactManager = contactManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var perms)) return GoToLogin();
        
        var canAdminUsers = perms.Contains(PermissionKeys.AdminUsers) || perms.Contains(PermissionKeys.AdminRoleMatrix);
        if (!canAdminUsers)
        {
            var role = JwtPermissionReader.GetClaim(token, "role") 
                       ?? JwtPermissionReader.GetClaim(token, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (role != "Admin" && role != "Manager") return RedirectToPage("/AccessDenied");
        }

        _apiClient.SetToken(token);

        Metrics = await _contactManager.GetMetricsAsync(ct);
        PendingContactChanges = await _contactManager.GetPendingAsync(ct);
        HistoricalContactChanges = await _contactManager.GetHistoryAsync(ct);

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid requestId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (Guid.TryParse(userIdStr, out var adminId))
        {
            try
            {
                var success = await _contactManager.ApproveAsync(requestId, adminId, ct);
                StatusMessage = success ? "Contact change request approved." : "Error: Failed to approve request.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid requestId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (Guid.TryParse(userIdStr, out var adminId))
        {
            try
            {
                var success = await _contactManager.RejectAsync(requestId, adminId, ct);
                StatusMessage = success ? "Contact change request rejected." : "Error: Failed to reject request.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        return RedirectToPage();
    }
}
