using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ReconciliationModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public DayEndReconciliationDto? Report { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateOnly SelectedDate { get; private set; } = DateOnly.FromDateTime(DateTime.Today);

    public ReconciliationModel(IApiClientService apiClient) => _apiClient = apiClient;

    public async Task<IActionResult> OnGetAsync(DateOnly? date, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.ReportsRead) &&
            !HasPermission(permissions, PermissionKeys.CashRead))
            return AccessDenied();

        _apiClient.SetToken(token);

        if (date.HasValue) SelectedDate = date.Value;

        try
        {
            Report = await _apiClient.GetAsync<DayEndReconciliationDto>(
                $"/api/cash/reconciliation?date={SelectedDate:yyyy-MM-dd}", ct);
        }
        catch
        {
            ErrorMessage = "Failed to load reconciliation data.";
        }

        return Page();
    }
}
