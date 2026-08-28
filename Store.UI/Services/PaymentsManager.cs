using System.Globalization;
using System.Text;
using Store.Models.DTOs.Payments;
using Store.Models.Enums;

namespace StoreUI.Services;

public class PaymentsManager : IPaymentsManager
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<PaymentsManager> _logger;

    public PaymentsManager(IApiClientService apiClient, ILogger<PaymentsManager> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<SettlementReportDto?> GetSettlementReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var url = $"/api/payments/settlement?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        return await _apiClient.GetAsync<SettlementReportDto>(url, ct);
    }

    public async Task<List<MobileMoneyTransactionDto>> GetTransactionsAsync(int page, int pageSize, MobileMoneyStatus? status = null, CancellationToken ct = default)
    {
        var statusParam = status.HasValue ? $"&status={(int)status.Value}" : string.Empty;
        var url = $"/api/payments/momo?page={page}&pageSize={pageSize}{statusParam}";
        return await _apiClient.GetAsync<List<MobileMoneyTransactionDto>>(url, ct) ?? new List<MobileMoneyTransactionDto>();
    }

    public async Task<MobileMoneyTransactionDto?> QueryTransactionStatusAsync(Guid transactionId, CancellationToken ct = default)
    {
        var url = $"/api/payments/momo/{transactionId}";
        return await _apiClient.GetAsync<MobileMoneyTransactionDto>(url, ct);
    }

    public byte[] GenerateSettlementCsv(SettlementReportDto? report, IEnumerable<MobileMoneyTransactionDto> transactions)
    {
        var sb = new StringBuilder();

        // ── Header Metadata ──
        sb.AppendLine("Architech Store ERP - Electronic Settlement & Payment Ledger");
        sb.AppendLine($"Generated UTC,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        if (report is not null)
        {
            sb.AppendLine($"Settlement Period,{report.FromDate:yyyy-MM-dd} to {report.ToDate:yyyy-MM-dd}");
            sb.AppendLine($"Total Settled Invoices,{report.TotalInvoices}");
            sb.AppendLine($"Total Electronic Sales (XAF),{report.TotalSales.ToString("F2", CultureInfo.InvariantCulture)}");
            sb.AppendLine();

            // ── Channel Breakdown ──
            sb.AppendLine("--- Channel Breakdown ---");
            sb.AppendLine("Payment Channel,Payment Type,Invoice Count,Total Amount (XAF)");
            foreach (var ch in report.ByChannel)
            {
                sb.AppendLine($"\"{EscapeCsv(ch.Channel)}\",\"{ch.PaymentType}\",{ch.InvoiceCount},{ch.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)}");
            }
            sb.AppendLine();
        }

        // ── Transaction Detailed Ledger ──
        sb.AppendLine("--- Mobile Money & Digital Transactions ---");
        sb.AppendLine("Transaction ID,Invoice ID,Provider,Phone Number,Amount (XAF),Status,Provider Reference,Date Created (UTC),Completed At (UTC)");

        foreach (var tx in transactions)
        {
            sb.AppendLine(string.Join(",",
                $"\"{tx.MobileMoneyTransactionId}\"",
                $"\"{tx.InvoiceId}\"",
                $"\"{EscapeCsv(tx.Provider)}\"",
                $"\"{EscapeCsv(tx.PhoneNumber)}\"",
                tx.Amount.ToString("F2", CultureInfo.InvariantCulture),
                $"\"{tx.Status}\"",
                $"\"{EscapeCsv(tx.ProviderTransactionId ?? "N/A")}\"",
                $"\"{tx.DateCreated:yyyy-MM-dd HH:mm:ss}\"",
                $"\"{(tx.CompletedAtUtc.HasValue ? tx.CompletedAtUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Pending")}\""
            ));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
