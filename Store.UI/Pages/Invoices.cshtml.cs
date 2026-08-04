using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class InvoicesModel : SecurePageModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<InvoiceDto> Invoices { get; private set; } = Array.Empty<InvoiceDto>();
    public InvoiceSummaryMetricsDto Summary { get; private set; } = new();

    public int TotalInvoices { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 20;
    public int TotalPages => (int)Math.Ceiling((double)TotalInvoices / PageSize);

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public string? PayType { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public string? DatePreset { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; } = "date_desc";
    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }
    [BindProperty(SupportsGet = true)] public string? Action { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public bool CanRefund { get; private set; }
    public bool CanVoid { get; private set; }
    public bool CanAddTender { get; private set; }

    public InvoicesModel(IInvoiceService invoiceService, IApiClientService apiClient)
    {
        _invoiceService = invoiceService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        _apiClient.SetToken(token);

        CanRefund = permissions.Contains("cash.write") || permissions.Contains("admin.users") || permissions.Contains("reports.read");
        CanVoid = permissions.Contains("cash.write") || permissions.Contains("admin.users");
        CanAddTender = permissions.Contains("cash.write");

        PageNumber = Math.Max(1, page);

        ResolveDateRange();

        PaymentType? parsedPayType = null;
        if (!string.IsNullOrWhiteSpace(PayType) && !PayType.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<PaymentType>(PayType, true, out var pt))
            {
                parsedPayType = pt;
            }
        }

        var request = new InvoicePagedRequest
        {
            Page = PageNumber,
            PageSize = PageSize,
            SearchTerm = Search?.Trim(),
            Status = Status?.Trim(),
            PaymentType = parsedPayType,
            FromDate = FromDate,
            ToDate = ToDate,
            SortBy = SortBy,
            SortDescending = SortBy?.EndsWith("_desc", StringComparison.OrdinalIgnoreCase) ?? true
        };

        var pagedTask = _invoiceService.GetAllAsync(request, ct);
        var summaryTask = _invoiceService.GetSummaryMetricsAsync(request, ct);

        await Task.WhenAll(pagedTask, summaryTask);

        var result = await pagedTask;
        Summary = await summaryTask;

        TotalInvoices = result.TotalCount;
        Invoices = result.Items?.ToList() ?? new List<InvoiceDto>();

        return Page();
    }

    private void ResolveDateRange()
    {
        var today = DateTime.Today;
        switch (DatePreset?.ToLowerInvariant())
        {
            case "today":
                FromDate = today;
                ToDate = today;
                break;
            case "yesterday":
                FromDate = today.AddDays(-1);
                ToDate = today.AddDays(-1);
                break;
            case "7days":
                FromDate = today.AddDays(-7);
                ToDate = today;
                break;
            case "30days":
                FromDate = today.AddDays(-30);
                ToDate = today;
                break;
            case "this_month":
                FromDate = new DateTime(today.Year, today.Month, 1);
                ToDate = today;
                break;
            case "custom":
                // Keep provided FromDate and ToDate
                break;
            default:
                // "all" or empty
                if (DatePreset == "all")
                {
                    FromDate = null;
                    ToDate = null;
                }
                break;
        }
    }

    /// <summary>Returns detailed invoice JSON for the slide-over drawer and print previews.</summary>
    public async Task<IActionResult> OnGetDetailAsync(Guid id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return Unauthorized();
        _apiClient.SetToken(token);

        var invoice = await _invoiceService.GetByIdAsync(id, ct);
        if (invoice is null) return NotFound(new { message = "Invoice not found." });

        return new JsonResult(invoice, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    public async Task<IActionResult> OnPostVoidAsync(Guid invoiceId, [FromForm] string? reason, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var success = await _invoiceService.VoidInvoiceAsync(invoiceId, null, reason, ct);
            StatusMessage = success ? "Invoice voided successfully." : "Error: Invoice not found or already voided.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error voiding invoice: {ex.Message}";
        }

        return RedirectToPage(new { page = PageNumber, search = Search, status = Status, payType = PayType, datePreset = DatePreset, fromDate = FromDate?.ToString("yyyy-MM-dd"), toDate = ToDate?.ToString("yyyy-MM-dd"), sortBy = SortBy });
    }

    public async Task<IActionResult> OnPostAddTenderAsync([FromQuery] Guid invoiceId, [FromBody] AddTenderRequest request, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return Unauthorized();
        _apiClient.SetToken(token);

        if (!ModelState.IsValid) return BadRequest(new { message = "Invalid payment tender data." });

        try
        {
            var tender = await _invoiceService.AddTenderAsync(invoiceId, request, ct);
            var updated = await _invoiceService.GetByIdAsync(invoiceId, ct);
            return new JsonResult(new { success = true, tender, invoice = updated }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Invoice not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostRefundAsync(Guid invoiceId, [FromBody] RefundInvoiceRequest request, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return Unauthorized();
        _apiClient.SetToken(token);

        if (!ModelState.IsValid) return BadRequest(new { message = "Invalid refund request payload." });

        try
        {
            var refundedInvoice = await _invoiceService.RefundInvoiceAsync(invoiceId, request, null, ct);
            if (refundedInvoice is null) return NotFound(new { message = "Invoice not found or cannot be refunded." });

            return new JsonResult(new { success = true, invoice = refundedInvoice, message = "Refund and return processed successfully." }, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return Unauthorized();
        _apiClient.SetToken(token);

        ResolveDateRange();

        PaymentType? parsedPayType = null;
        if (!string.IsNullOrWhiteSpace(PayType) && !PayType.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<PaymentType>(PayType, true, out var pt)) parsedPayType = pt;
        }

        var request = new InvoicePagedRequest
        {
            Page = 1,
            PageSize = 5000,
            SearchTerm = Search?.Trim(),
            Status = Status?.Trim(),
            PaymentType = parsedPayType,
            FromDate = FromDate,
            ToDate = ToDate,
            SortBy = SortBy,
            SortDescending = SortBy?.EndsWith("_desc", StringComparison.OrdinalIgnoreCase) ?? true
        };

        var result = await _invoiceService.GetAllAsync(request, ct);
        var items = result.Items ?? Enumerable.Empty<InvoiceDto>();

        var sb = new StringBuilder();
        // UTF-8 BOM
        sb.Append('\uFEFF');
        sb.AppendLine("Invoice ID,Date,Customer,Phone,Cashier,Branch,Payment Type,Total Amount (XAF),Amount Tendered (XAF),Outstanding (XAF),Status,Refunded,Notes");

        foreach (var i in items)
        {
            var statusStr = i.IsPaid ? "Paid" : (i.AmountTendered == 0 ? "Voided" : "Partial Debt");
            var refStr = i.IsRefunded ? $"Yes ({i.RefundedAmount:N0})" : "No";

            sb.AppendLine($"\"{i.InvoiceId}\",\"{i.DateCreated:yyyy-MM-dd HH:mm}\",\"{EscapeCsv(i.CustomerName ?? "Walk-in")}\",\"{EscapeCsv(i.CustomerPhone ?? "")}\",\"{EscapeCsv(i.ProcessedBy ?? "")}\",\"{EscapeCsv(i.BranchName ?? "")}\",\"{EscapeCsv(i.TenderSummary)}\",{i.TotalAmount},{i.AmountTendered},{i.OutstandingBalance},\"{statusStr}\",\"{refStr}\",\"{EscapeCsv(i.Notes ?? "")}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"Invoices_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static string EscapeCsv(string val) => val.Replace("\"", "\"\"");
}

