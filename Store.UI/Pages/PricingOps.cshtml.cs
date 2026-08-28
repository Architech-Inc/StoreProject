using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PricingOpsModel : SecurePageModel
{
    private readonly IPricingOpsManager _pricingManager;
    private readonly IApiClientService _apiClient;

    public PricingOpsMetricsDto Metrics { get; private set; } = new();
    public List<TaxProfileDto> TaxProfiles { get; private set; } = new();
    public List<BundleRuleDto> BundleRules { get; private set; } = new();
    public List<SegmentPricingDto> SegmentPricings { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "simulator";

    [BindProperty]
    public UpsertTaxProfileRequest TaxRequest { get; set; } = new();

    [BindProperty]
    public UpsertBundleRuleRequest BundleRequest { get; set; } = new();

    [BindProperty]
    public UpsertSegmentPricingRequest SegmentRequest { get; set; } = new();

    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public PricingOpsModel(
        IPricingOpsManager pricingManager,
        IApiClientService apiClient)
    {
        _pricingManager = pricingManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        CanRead = HasPermission(permissions, PermissionKeys.PricingRead) ||
                  HasPermission(permissions, PermissionKeys.InventoryRead);
        CanWrite = HasPermission(permissions, PermissionKeys.PricingWrite);

        if (!CanRead)
            return AccessDenied();

        Metrics = await _pricingManager.GetMetricsAsync(ct);
        TaxProfiles = await _pricingManager.GetTaxProfilesAsync(ct);
        BundleRules = await _pricingManager.GetBundleRulesAsync(ct);
        SegmentPricings = await _pricingManager.GetSegmentPricingsAsync(ct);

        return Page();
    }

    public async Task<IActionResult> OnGetSimulateAsync(
        [FromQuery] Guid itemId,
        [FromQuery] int quantity,
        [FromQuery] CustomerSegment segment,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        if (itemId == Guid.Empty)
            return BadRequest(new { message = "Valid Item ID is required." });

        var req = new PricingPreviewRequest
        {
            ItemId = itemId,
            Quantity = quantity < 1 ? 1 : quantity,
            Segment = segment
        };

        var preview = await _pricingManager.GetPricingPreviewAsync(req, ct);
        if (preview is null)
            return NotFound(new { message = "Item pricing profile not found." });

        return new JsonResult(preview);
    }

    public async Task<IActionResult> OnGetSearchItemsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var items = await _pricingManager.SearchItemsAsync(q, ct);
        var list = items.Select(i => new
        {
            id = i.ItemId.ToString(),
            title = i.Name,
            sub = $"Barcode: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | Price: {i.UnitPrice:N0} XAF | Cost: {i.CostPrice:N0} XAF",
            unitPrice = i.UnitPrice,
            costPrice = i.CostPrice,
            badge = i.CategoryName ?? "General"
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnGetExportCsvAsync([FromQuery] string? type, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var targetType = (type ?? "segments").ToLowerInvariant();
        byte[] bytes;
        string filename;

        if (targetType == "taxes")
        {
            var list = await _pricingManager.GetTaxProfilesAsync(ct);
            bytes = _pricingManager.ExportTaxesCsv(list);
            filename = $"tax_profiles_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        }
        else if (targetType == "bundles")
        {
            var list = await _pricingManager.GetBundleRulesAsync(ct);
            bytes = _pricingManager.ExportBundlesCsv(list);
            filename = $"bundle_rules_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        }
        else
        {
            var list = await _pricingManager.GetSegmentPricingsAsync(ct);
            bytes = _pricingManager.ExportSegmentsCsv(list);
            filename = $"segment_pricings_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        }

        return File(bytes, "text/csv", filename);
    }

    public async Task<IActionResult> OnPostTaxAsync(CancellationToken ct = default)
    {
        return await HandleWriteAsync(async () =>
        {
            var res = await _pricingManager.UpsertTaxProfileAsync(TaxRequest, ct);
            StatusMessage = $"Tax profile '{res.Name}' ({res.RatePercent}% {res.ApplicationType}) saved successfully.";
        }, "taxes");
    }

    public async Task<IActionResult> OnPostBundleAsync(CancellationToken ct = default)
    {
        return await HandleWriteAsync(async () =>
        {
            var res = await _pricingManager.UpsertBundleRuleAsync(BundleRequest, ct);
            StatusMessage = $"Bundle combo rule '{res.Name}' saved successfully.";
        }, "bundles");
    }

    public async Task<IActionResult> OnPostSegmentAsync(CancellationToken ct = default)
    {
        return await HandleWriteAsync(async () =>
        {
            var res = await _pricingManager.UpsertSegmentPricingAsync(SegmentRequest, ct);
            StatusMessage = $"Customer tier pricing override for '{res.ItemName}' ({res.Segment}: {res.PriceOverride:N0} XAF) saved.";
        }, "segments");
    }

    private async Task<IActionResult> HandleWriteAsync(Func<Task> operation, string tab)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.PricingWrite))
        {
            StatusMessage = "Error: You do not have permission to modify pricing rules.";
            return RedirectToPage(new { ActiveTab = tab });
        }

        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to save pricing rule - {ex.Message}";
        }

        return RedirectToPage(new { ActiveTab = tab });
    }
}
