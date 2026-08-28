using System.Text;
using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public class CashReportsManager : ICashReportsManager
{
    private readonly IApiClientService _apiClient;

    public CashReportsManager(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<CashierShiftDto?> GetActiveShiftAsync(CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<CashierShiftDto>("/api/cash/shift/active", ct);
    }

    public async Task<List<CashierShiftDto>> GetShiftsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<List<CashierShiftDto>>($"/api/cash/shifts?page={page}&pageSize={pageSize}", ct)
            ?? new List<CashierShiftDto>();
    }

    public async Task<CashierShiftDto?> OpenShiftAsync(ShiftOpenRequest request, CancellationToken ct = default)
    {
        return await _apiClient.PostAsync<CashierShiftDto>("/api/cash/shift/open", request, ct);
    }

    public async Task<CashierShiftDto?> CloseShiftAsync(ShiftCloseRequest request, CancellationToken ct = default)
    {
        return await _apiClient.PostAsync<CashierShiftDto>("/api/cash/shift/close", request, ct);
    }

    public async Task<DailyZReportDto?> GetDailyZReportAsync(DateTime dateUtc, CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<DailyZReportDto>($"/api/cash/report/z?dateUtc={dateUtc:O}", ct);
    }

    public byte[] GenerateZReportCsv(DailyZReportDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# ClexAn Foods - Official Daily Z-Report ({report.Date:yyyy-MM-dd})");
        sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC | Currency: XAF");
        sb.AppendLine();
        sb.AppendLine("FISCAL SUMMARY");
        sb.AppendLine($"Gross Sales (XAF),{report.GrossSales:N0}");
        sb.AppendLine($"Discounts Given (XAF),{report.Discounts:N0}");
        sb.AppendLine($"Net Sales (XAF),{report.NetSales:N0}");
        sb.AppendLine($"Cost of Goods Sold - COGS (XAF),{report.Cogs:N0}");
        sb.AppendLine($"Gross Margin (XAF),{report.GrossMargin:N0}");
        var marginPct = report.NetSales > 0 ? (report.GrossMargin / report.NetSales * 100) : 0;
        sb.AppendLine($"Gross Margin %,{marginPct:F1}%");
        sb.AppendLine($"Total Invoices,{report.InvoiceCount}");
        sb.AppendLine($"Average Basket Size (XAF),{report.AverageBasket:N0}");
        sb.AppendLine();

        sb.AppendLine("PAYMENT BREAKDOWN");
        sb.AppendLine("Payment Method,Total Amount (XAF),Invoice Count,Share %");
        foreach (var p in report.PaymentBreakdown)
        {
            var share = report.GrossSales > 0 ? (p.TotalAmount / report.GrossSales * 100) : 0;
            sb.AppendLine($"{p.PaymentType},{p.TotalAmount:N0},{p.InvoiceCount},{share:F1}%");
        }
        sb.AppendLine();

        sb.AppendLine("TOP PERFORMING PRODUCTS");
        sb.AppendLine("Item Name,Quantity Sold,Revenue (XAF),Gross Margin (XAF),Margin %");
        foreach (var prod in report.TopProducts)
        {
            var pMarginPct = prod.Revenue > 0 ? (prod.GrossMargin / prod.Revenue * 100) : 0;
            sb.AppendLine($"\"{prod.ItemName}\",{prod.QuantitySold},{prod.Revenue:N0},{prod.GrossMargin:N0},{pMarginPct:F1}%");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
