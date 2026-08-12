using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Users;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ContactRequestsModel : SecurePageModel
{
    private readonly IUserService _userService;
    private readonly IApiClientService _apiClient;

    public IReadOnlyCollection<ContactChangeRequestDto> PendingContactChanges { get; private set; } = Array.Empty<ContactChangeRequestDto>();
    public IReadOnlyCollection<ContactChangeRequestDto> HistoricalContactChanges { get; private set; } = Array.Empty<ContactChangeRequestDto>();

    [TempData] public string? StatusMessage { get; set; }

    public ContactRequestsModel(IUserService userService, IApiClientService apiClient)
    {
        _userService = userService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        
        var role = JwtPermissionReader.GetClaim(token, "role") 
                   ?? JwtPermissionReader.GetClaim(token, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                   
        if (role != "Admin" && role != "Manager") return RedirectToPage("/AccessDenied");

        _apiClient.SetToken(token);

        var pendingChanges = await _userService.GetPendingContactChangesAsync(ct);
        PendingContactChanges = pendingChanges.Where(p => p.Status == ContactChangeStatus.PendingApproval || p.Status == ContactChangeStatus.PendingVerification).ToList();

        HistoricalContactChanges = await _userService.GetContactChangeHistoryAsync(ct);

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
                var success = await _userService.ApproveContactChangeAsync(requestId, adminId, ct);
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
                var success = await _userService.RejectContactChangeAsync(requestId, adminId, ct);
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
