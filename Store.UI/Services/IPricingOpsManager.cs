using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public interface IPricingOpsManager
{
    Task<PricingOpsMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<List<TaxProfileDto>> GetTaxProfilesAsync(CancellationToken ct = default);
    Task<TaxProfileDto> UpsertTaxProfileAsync(UpsertTaxProfileRequest request, CancellationToken ct = default);
    Task<List<BundleRuleDto>> GetBundleRulesAsync(CancellationToken ct = default);
    Task<BundleRuleDto> UpsertBundleRuleAsync(UpsertBundleRuleRequest request, CancellationToken ct = default);
    Task<List<SegmentPricingDto>> GetSegmentPricingsAsync(CancellationToken ct = default);
    Task<SegmentPricingDto> UpsertSegmentPricingAsync(UpsertSegmentPricingRequest request, CancellationToken ct = default);
    Task<PricingPreviewDto?> GetPricingPreviewAsync(PricingPreviewRequest request, CancellationToken ct = default);
    Task<List<ItemDto>> SearchItemsAsync(string? query, CancellationToken ct = default);
    byte[] ExportTaxesCsv(IEnumerable<TaxProfileDto> taxes);
    byte[] ExportBundlesCsv(IEnumerable<BundleRuleDto> bundles);
    byte[] ExportSegmentsCsv(IEnumerable<SegmentPricingDto> segments);
}
