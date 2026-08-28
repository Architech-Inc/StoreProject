using System.Text;
using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public class ReconciliationManager : IReconciliationManager
{
    private readonly IApiClientService _apiClient;

    public ReconciliationManager(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<DayEndReconciliationDto?> GetDayEndReconciliationAsync(DateOnly date, CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<DayEndReconciliationDto>($"/api/cash/reconciliation?date={date:yyyy-MM-dd}", ct);
    }

    public byte[] GenerateReconciliationCsv(DayEndReconciliationDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# ClexAn Foods - Day-End Reconciliation Report ({report.Date:yyyy-MM-dd})");
        sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC | Currency: XAF");
        sb.AppendLine();
        sb.AppendLine("SUMMARY");
        sb.AppendLine($"Total Shifts,{report.TotalShifts}");
        sb.AppendLine($"Open Shifts,{report.OpenShifts}");
        sb.AppendLine($"Total Cash Sales (XAF),{report.TotalCashSales:N0}");
        sb.AppendLine($"Total Non-Cash Sales (XAF),{report.TotalNonCashSales:N0}");
        sb.AppendLine($"Total Combined Sales (XAF),{(report.TotalCashSales + report.TotalNonCashSales):N0}");
        sb.AppendLine($"Total Variance (XAF),{report.TotalVariance:N0}");
        sb.AppendLine();

        sb.AppendLine("CASHIER SHIFT AUDIT DETAILS");
        sb.AppendLine("Shift ID,Cashier Name,Status,Opened (UTC),Closed (UTC),Opening Float (XAF),Closing Float (XAF),Expected Close (XAF),Variance (XAF),Cash Sales (XAF),Invoices Count");

        foreach (var s in report.Shifts)
        {
            var openedStr = s.OpenedAtUtc.ToString("yyyy-MM-dd HH:mm");
            var closedStr = s.ClosedAtUtc.HasValue ? s.ClosedAtUtc.Value.ToString("yyyy-MM-dd HH:mm") : "Ongoing";
            var closingFloat = s.ClosingFloat.HasValue ? s.ClosingFloat.Value.ToString("N0") : "—";
            var expectedClose = s.ExpectedClosingAmount.HasValue ? s.ExpectedClosingAmount.Value.ToString("N0") : "—";
            var variance = s.VarianceAmount.HasValue ? s.VarianceAmount.Value.ToString("N0") : "—";

            sb.AppendLine($"\"{s.CashierShiftId}\",\"{s.CashierName}\",{s.Status},\"{openedStr}\",\"{closedStr}\",{s.OpeningFloat:N0},{closingFloat},{expectedClose},{variance},{s.CashSalesTotal:N0},{s.InvoiceCount}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
