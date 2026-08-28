using System.Text;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class PricingOpsManager : IPricingOpsManager
{
    private readonly IApiClientService _apiClient;
    private readonly IItemService _itemService;

    public PricingOpsManager(IApiClientService apiClient, IItemService itemService)
    {
        _apiClient = apiClient;
        _itemService = itemService;
    }

    public async Task<PricingOpsMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _apiClient.GetAsync<PricingOpsMetricsDto>("/api/pricing/metrics", ct) ?? new();

    public async Task<List<TaxProfileDto>> GetTaxProfilesAsync(CancellationToken ct = default)
        => await _apiClient.GetAsync<List<TaxProfileDto>>("/api/pricing/tax-profiles", ct) ?? new();

    public async Task<TaxProfileDto> UpsertTaxProfileAsync(UpsertTaxProfileRequest request, CancellationToken ct = default)
    {
        var result = await _apiClient.PostAsync<TaxProfileDto>("/api/pricing/tax-profiles", request, ct);
        return result ?? throw new InvalidOperationException("Failed to save tax profile.");
    }

    public async Task<List<BundleRuleDto>> GetBundleRulesAsync(CancellationToken ct = default)
        => await _apiClient.GetAsync<List<BundleRuleDto>>("/api/pricing/bundles", ct) ?? new();

    public async Task<BundleRuleDto> UpsertBundleRuleAsync(UpsertBundleRuleRequest request, CancellationToken ct = default)
    {
        var result = await _apiClient.PostAsync<BundleRuleDto>("/api/pricing/bundles", request, ct);
        return result ?? throw new InvalidOperationException("Failed to save bundle rule.");
    }

    public async Task<List<SegmentPricingDto>> GetSegmentPricingsAsync(CancellationToken ct = default)
        => await _apiClient.GetAsync<List<SegmentPricingDto>>("/api/pricing/segment-pricing", ct) ?? new();

    public async Task<SegmentPricingDto> UpsertSegmentPricingAsync(UpsertSegmentPricingRequest request, CancellationToken ct = default)
    {
        var result = await _apiClient.PostAsync<SegmentPricingDto>("/api/pricing/segment-pricing", request, ct);
        return result ?? throw new InvalidOperationException("Failed to save segment pricing.");
    }

    public async Task<PricingPreviewDto?> GetPricingPreviewAsync(PricingPreviewRequest request, CancellationToken ct = default)
        => await _apiClient.PostAsync<PricingPreviewDto>("/api/pricing/preview", request, ct);

    public async Task<List<ItemDto>> SearchItemsAsync(string? query, CancellationToken ct = default)
    {
        var req = new PagedRequest { Page = 1, PageSize = 25, SearchTerm = query?.Trim() };
        var result = await _itemService.GetAllAsync(req, ct);
        return result.Items.ToList();
    }

    public byte[] ExportTaxesCsv(IEnumerable<TaxProfileDto> taxes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Tax Profile ID,Name,Rate Percent,Application Type,Status");
        foreach (var t in taxes)
        {
            sb.AppendLine($"{t.TaxProfileId},\"{t.Name.Replace("\"", "\"\"")}\",{t.RatePercent},{t.ApplicationType},{(t.IsActive ? "Active" : "Inactive")}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportBundlesCsv(IEnumerable<BundleRuleDto> bundles)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bundle Rule ID,Rule Name,Trigger Product,Trigger Qty,Reward Product,Reward Qty,Reward Discount %,Valid From,Valid To,Status");
        foreach (var b in bundles)
        {
            sb.AppendLine($"{b.BundleRuleId},\"{b.Name.Replace("\"", "\"\"")}\",\"{b.TriggerItemName.Replace("\"", "\"\"")}\",{b.TriggerQuantity},\"{b.RewardItemName.Replace("\"", "\"\"")}\",{b.RewardQuantity},{b.RewardDiscountPercent},\"{b.ValidFrom:yyyy-MM-dd}\",\"{b.ValidTo:yyyy-MM-dd}\",{(b.IsActive ? "Active" : "Inactive")}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportSegmentsCsv(IEnumerable<SegmentPricingDto> segments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Segment Price ID,Product Name,Base Catalog Price (XAF),Unit Cost (XAF),Customer Tier,Override Price (XAF),Profit Margin %,Valid From,Valid To,Status");
        foreach (var s in segments)
        {
            sb.AppendLine($"{s.CustomerSegmentPriceId},\"{s.ItemName.Replace("\"", "\"\"")}\",{s.BaseUnitPrice},{s.UnitCostPrice},{s.Segment},{s.PriceOverride},{s.MarginPercent}%,\"{s.ValidFrom:yyyy-MM-dd}\",\"{s.ValidTo:yyyy-MM-dd}\",{(s.IsActive ? "Active" : "Inactive")}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
