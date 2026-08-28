using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PromotionEffectivenessModel : SecurePageModel
{
    private readonly IPromotionEffectivenessManager _promoManager;
    private readonly IApiClientService _apiClient;

    public PromotionEffectivenessDto? Report { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime ToDate { get; private set; }
    public string ActivePreset { get; private set; } = "30d";
    public string ActiveTab { get; private set; } = "overview";

    [TempData] public string? StatusMessage { get; set; }

    public PromotionEffectivenessModel(
        IPromotionEffectivenessManager promoManager,
        IApiClientService apiClient)
    {
        _promoManager = promoManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? preset = null,
        string? tab = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.PricingRead))
            return AccessDenied();

        _apiClient.SetToken(token);

        ActiveTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow.Date;

        if (!string.IsNullOrWhiteSpace(preset))
        {
            ActivePreset = preset.Trim().ToLowerInvariant();
            switch (ActivePreset)
            {
                case "today":
                    FromDate = now;
                    ToDate = now;
                    break;
                case "yesterday":
                    FromDate = now.AddDays(-1);
                    ToDate = now.AddDays(-1);
                    break;
                case "7d":
                    FromDate = now.AddDays(-7);
                    ToDate = now;
                    break;
                case "30d":
                    FromDate = now.AddDays(-30);
                    ToDate = now;
                    break;
                case "month":
                    FromDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    ToDate = now;
                    break;
                case "quarter":
                    FromDate = now.AddMonths(-3);
                    ToDate = now;
                    break;
                case "all":
                    FromDate = now.AddYears(-3);
                    ToDate = now;
                    break;
                default:
                    FromDate = from?.Date ?? now.AddDays(-30);
                    ToDate = to?.Date ?? now;
                    ActivePreset = "custom";
                    break;
            }
        }
        else if (from.HasValue || to.HasValue)
        {
            FromDate = from?.Date ?? now.AddDays(-30);
            ToDate = to?.Date ?? now;
            ActivePreset = "custom";
        }
        else
        {
            ActivePreset = "30d";
            FromDate = now.AddDays(-30);
            ToDate = now;
        }

        if (ToDate < FromDate)
        {
            ToDate = FromDate;
        }

        Report = await _promoManager.GetEffectivenessReportAsync(FromDate, ToDate, ct);
        return Page();
    }

    public async Task<IActionResult> OnGetExportCsvAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? section = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        if (!HasPermission(permissions, PermissionKeys.PricingRead))
            return Forbid();

        _apiClient.SetToken(token);

        var fromDt = from?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var toDt = to?.Date ?? DateTime.UtcNow.Date;
        var report = await _promoManager.GetEffectivenessReportAsync(fromDt, toDt, ct);

        if (report is null)
            return NotFound("Report data could not be retrieved.");

        var csvBytes = _promoManager.GenerateCsv(report, section ?? "all");
        var fileName = $"promotion_effectiveness_{fromDt:yyyyMMdd}_{toDt:yyyyMMdd}.csv";

        return File(csvBytes, "text/csv", fileName);
    }
}
