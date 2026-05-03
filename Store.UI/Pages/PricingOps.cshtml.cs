using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PricingOpsModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;
    private readonly IItemService _itemService;

    public IReadOnlyList<ItemDto> Items { get; private set; } = Array.Empty<ItemDto>();
    public IReadOnlyList<TaxProfileDto> TaxProfiles { get; private set; } = Array.Empty<TaxProfileDto>();
    public IReadOnlyList<BundleRuleDto> BundleRules { get; private set; } = Array.Empty<BundleRuleDto>();
    public IReadOnlyList<SegmentPricingDto> SegmentPricings { get; private set; } = Array.Empty<SegmentPricingDto>();

    [BindProperty] public string TaxName { get; set; } = string.Empty;
    [BindProperty] public decimal TaxRate { get; set; }
    [BindProperty] public TaxApplicationType TaxApplicationType { get; set; }

    [BindProperty] public string BundleName { get; set; } = string.Empty;
    [BindProperty] public Guid BundleTriggerItemId { get; set; }
    [BindProperty] public Guid BundleRewardItemId { get; set; }
    [BindProperty] public int BundleTriggerQty { get; set; } = 2;
    [BindProperty] public int BundleRewardQty { get; set; } = 1;
    [BindProperty] public decimal BundleDiscountPercent { get; set; } = 10;

    [BindProperty] public Guid SegmentItemId { get; set; }
    [BindProperty] public CustomerSegment Segment { get; set; }
    [BindProperty] public decimal SegmentPrice { get; set; }

    [BindProperty] public Guid PreviewItemId { get; set; }
    [BindProperty] public int PreviewQty { get; set; } = 1;
    [BindProperty] public CustomerSegment PreviewSegment { get; set; }

    public PricingPreviewDto? Preview { get; private set; }

    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public PricingOpsModel(IApiClientService apiClient, IItemService itemService)
    {
        _apiClient = apiClient;
        _itemService = itemService;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        CanRead = HasPermission(permissions, PermissionKeys.PricingRead);
        CanWrite = HasPermission(permissions, PermissionKeys.PricingWrite);

        if (!CanRead)
        {
            return AccessDenied();
        }

        await LoadAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostTaxAsync(CancellationToken ct)
    {
        return await HandleWriteAsync(async () =>
        {
            var req = new UpsertTaxProfileRequest
            {
                Name = TaxName,
                RatePercent = TaxRate,
                ApplicationType = TaxApplicationType,
                IsActive = true
            };
            await _apiClient.PostAsync<TaxProfileDto>("/api/pricing/tax-profiles", req, ct);
            StatusMessage = "Tax profile saved.";
        });
    }

    public async Task<IActionResult> OnPostBundleAsync(CancellationToken ct)
    {
        return await HandleWriteAsync(async () =>
        {
            var req = new UpsertBundleRuleRequest
            {
                Name = BundleName,
                TriggerItemId = BundleTriggerItemId,
                RewardItemId = BundleRewardItemId,
                TriggerQuantity = BundleTriggerQty,
                RewardQuantity = BundleRewardQty,
                RewardDiscountPercent = BundleDiscountPercent,
                IsActive = true
            };
            await _apiClient.PostAsync<BundleRuleDto>("/api/pricing/bundles", req, ct);
            StatusMessage = "Bundle rule saved.";
        });
    }

    public async Task<IActionResult> OnPostSegmentAsync(CancellationToken ct)
    {
        return await HandleWriteAsync(async () =>
        {
            var req = new UpsertSegmentPricingRequest
            {
                ItemId = SegmentItemId,
                Segment = Segment,
                PriceOverride = SegmentPrice,
                IsActive = true
            };
            await _apiClient.PostAsync<SegmentPricingDto>("/api/pricing/segment-pricing", req, ct);
            StatusMessage = "Segment pricing saved.";
        });
    }

    public async Task<IActionResult> OnPostPreviewAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.PricingRead))
        {
            return AccessDenied();
        }

        var req = new PricingPreviewRequest
        {
            ItemId = PreviewItemId,
            Quantity = PreviewQty,
            Segment = PreviewSegment
        };

        Preview = await _apiClient.PostAsync<PricingPreviewDto>("/api/pricing/preview", req, ct);
        CanRead = true;
        CanWrite = HasPermission(permissions, PermissionKeys.PricingWrite);
        await LoadAsync(ct);
        return Page();
    }

    private async Task<IActionResult> HandleWriteAsync(Func<Task> operation)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.PricingWrite))
        {
            return AccessDenied();
        }

        await operation();
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        var items = await _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 200 }, ct);
        Items = items.Items.OrderBy(x => x.Name).ToList();

        TaxProfiles = await _apiClient.GetAsync<List<TaxProfileDto>>("/api/pricing/tax-profiles", ct)
            ?? new List<TaxProfileDto>();

        BundleRules = await _apiClient.GetAsync<List<BundleRuleDto>>("/api/pricing/bundles", ct)
            ?? new List<BundleRuleDto>();

        SegmentPricings = await _apiClient.GetAsync<List<SegmentPricingDto>>("/api/pricing/segment-pricing", ct)
            ?? new List<SegmentPricingDto>();
    }
}
