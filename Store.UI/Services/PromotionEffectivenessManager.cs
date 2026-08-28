using System.Text;
using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public class PromotionEffectivenessManager : IPromotionEffectivenessManager
{
    private readonly IApiClientService _apiClient;

    public PromotionEffectivenessManager(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<PromotionEffectivenessDto?> GetEffectivenessReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var url = $"/api/pricing/promotions/effectiveness?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        return await _apiClient.GetAsync<PromotionEffectivenessDto>(url, ct);
    }

    public byte[] GenerateCsv(PromotionEffectivenessDto report, string section)
    {
        var sec = (section ?? "all").ToLowerInvariant();
        var sb = new StringBuilder();

        sb.AppendLine($"# ClexAn Foods - Promotion Effectiveness Report ({report.FromDate:yyyy-MM-dd} to {report.ToDate:yyyy-MM-dd})");
        sb.AppendLine($"# Total Gross Revenue (XAF): {report.TotalGrossRevenue:N0}");
        sb.AppendLine($"# Total Discount Investment (XAF): {report.TotalDiscountGiven:N0}");
        sb.AppendLine($"# Total Net Revenue (XAF): {report.TotalNetRevenue:N0}");
        sb.AppendLine($"# Discount Penetration: {report.DiscountPenetrationRatePercent}% ({report.InvoicesWithDiscountCount}/{report.TotalInvoicesCount} invoices)");
        sb.AppendLine($"# Estimated Gross Margin: {report.EstimatedGrossMarginPercent}%");
        sb.AppendLine();

        if (sec == "rules" || sec == "all")
        {
            sb.AppendLine("## Promotional Rules Breakdown");
            sb.AppendLine("Discount ID,Rule Name,Coupon Code,Discount Type,Value,Redemptions,Gross Revenue (XAF),Discount Given (XAF)");
            foreach (var r in report.RulesSummary)
            {
                sb.AppendLine($"{r.DiscountId},\"{r.Name.Replace("\"", "\"\"")}\",\"{r.CouponCode ?? ""}\",{r.DiscountType},\"{r.ValueFormatted}\",{r.RedemptionsCount},{r.TotalRevenue:N0},{r.TotalDiscountGiven:N0}");
            }
            sb.AppendLine();
        }

        if (sec == "items" || sec == "all")
        {
            sb.AppendLine("## Top Discounted Items");
            sb.AppendLine("Item ID,Product Name,Category,Unit Cost (XAF),Unit Retail (XAF),Discount %,Units Sold,Revenue (XAF),Discount Given (XAF),Gross Margin %,Loss Leader");
            foreach (var item in report.TopDiscountedItems)
            {
                sb.AppendLine($"{item.ItemId},\"{item.ItemName.Replace("\"", "\"\"")}\",\"{item.CategoryName ?? "—"}\",{item.UnitCostPrice:N0},{item.UnitSellingPrice:N0},{item.DiscountPercent}%,{item.UnitsSold},{item.TotalRevenue:N0},{item.TotalDiscountGiven:N0},{item.GrossMarginPercent}%,{(item.IsLossLeader ? "YES" : "NO")}");
            }
            sb.AppendLine();
        }

        if (sec == "bundles" || sec == "all")
        {
            sb.AppendLine("## Bundle & BOGO Deals");
            sb.AppendLine("Bundle ID,Rule Name,Trigger Product,Trigger Qty,Reward Product,Reward Qty,Reward Discount %,Qualifying Invoices,Estimated Savings (XAF)");
            foreach (var b in report.BundleHits)
            {
                sb.AppendLine($"{b.BundleRuleId},\"{b.BundleName.Replace("\"", "\"\"")}\",\"{b.TriggerItemName.Replace("\"", "\"\"")}\",{b.TriggerQuantity},\"{b.RewardItemName.Replace("\"", "\"\"")}\",{b.RewardQuantity},{b.RewardDiscountPercent}%,{b.TriggerInvoiceCount},{b.EstimatedSavingsXaf:N0}");
            }
            sb.AppendLine();
        }

        if (sec == "segments" || sec == "all")
        {
            sb.AppendLine("## Customer Tier Pricing");
            sb.AppendLine("Customer Tier,Product Name,Category,Unit Cost (XAF),Standard Retail (XAF),Tier Price (XAF),Unit Savings (XAF),Units Sold,Revenue (XAF),Total Savings (XAF),Gross Margin %");
            foreach (var seg in report.SegmentSummary)
            {
                sb.AppendLine($"{seg.Segment},\"{seg.ItemName.Replace("\"", "\"\"")}\",\"{seg.CategoryName ?? "—"}\",{seg.UnitCostPrice:N0},{seg.StandardPrice:N0},{seg.SegmentPrice:N0},{seg.UnitSavings:N0},{seg.UnitsSold},{seg.TotalRevenue:N0},{seg.TotalSavingsXaf:N0},{seg.GrossMarginPercent}%");
            }
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
