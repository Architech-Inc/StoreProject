using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PromotionEffectivenessModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public PromotionEffectivenessDto? Report { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime ToDate { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public PromotionEffectivenessModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.PricingRead))
            return AccessDenied();

        _apiClient.SetToken(token);

        FromDate = from?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        ToDate = to?.Date ?? DateTime.UtcNow.Date;

        var url = $"/api/pricing/promotions/effectiveness?fromDate={FromDate:yyyy-MM-dd}&toDate={ToDate:yyyy-MM-dd}";
        Report = await _apiClient.GetAsync<PromotionEffectivenessDto>(url, ct);

        return Page();
    }
}
