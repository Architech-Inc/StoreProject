using System.Text;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class InvoiceManager : IInvoiceManager
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceManager(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public async Task<PagedResult<InvoiceDto>> GetInvoicesPagedAsync(InvoicePagedRequest request, CancellationToken ct = default)
    {
        return await _invoiceService.GetAllAsync(request, ct);
    }

    public async Task<InvoiceSummaryMetricsDto> GetSummaryMetricsAsync(InvoicePagedRequest request, CancellationToken ct = default)
    {
        return await _invoiceService.GetSummaryMetricsAsync(request, ct);
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _invoiceService.GetByIdAsync(invoiceId, ct);
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, string? reason, CancellationToken ct = default)
    {
        return await _invoiceService.VoidInvoiceAsync(invoiceId, null, reason, ct);
    }

    public async Task<InvoiceTenderDto> AddTenderAsync(Guid invoiceId, AddTenderRequest request, CancellationToken ct = default)
    {
        return await _invoiceService.AddTenderAsync(invoiceId, request, ct);
    }

    public async Task<InvoiceDto?> RefundInvoiceAsync(Guid invoiceId, RefundInvoiceRequest request, CancellationToken ct = default)
    {
        return await _invoiceService.RefundInvoiceAsync(invoiceId, request, null, ct);
    }

    public byte[] GenerateInvoicesCsv(IEnumerable<InvoiceDto> items)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF'); // UTF-8 BOM
        sb.AppendLine("Invoice ID,Date,Customer,Phone,Cashier,Branch,Payment Type,Total Amount (XAF),Amount Tendered (XAF),Outstanding (XAF),Status,Refunded,Notes");

        foreach (var i in items)
        {
            var statusStr = i.IsPaid ? "Paid" : (i.AmountTendered == 0 ? "Voided" : "Partial Debt");
            var refStr = i.IsRefunded ? $"Yes ({i.RefundedAmount:N0})" : "No";

            sb.AppendLine($"\"{i.InvoiceId}\",\"{i.DateCreated:yyyy-MM-dd HH:mm}\",\"{EscapeCsv(i.CustomerName ?? "Walk-in")}\",\"{EscapeCsv(i.CustomerPhone ?? "")}\",\"{EscapeCsv(i.ProcessedBy ?? "")}\",\"{EscapeCsv(i.BranchName ?? "")}\",\"{EscapeCsv(i.TenderSummary)}\",{i.TotalAmount},{i.AmountTendered},{i.OutstandingBalance},\"{statusStr}\",\"{refStr}\",\"{EscapeCsv(i.Notes ?? "")}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string val) => val.Replace("\"", "\"\"");
}
