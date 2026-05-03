using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class InvoicesModel : SecurePageModel
{
    private readonly IInvoiceService _invoiceService;

    public IReadOnlyList<InvoiceDto> Invoices { get; private set; } = Array.Empty<InvoiceDto>();
    public int TotalInvoices { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalInvoices / PageSize);

    public string SearchTerm { get; private set; } = string.Empty;
    public string PayTypeFilter { get; private set; } = string.Empty;

    [TempData] public string? StatusMessage { get; set; }

    public InvoicesModel(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, string? search = null, string? payType = null, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        PageNumber = Math.Max(1, page);
        SearchTerm = search ?? string.Empty;
        PayTypeFilter = payType ?? string.Empty;

        var result = await _invoiceService.GetAllAsync(new PagedRequest { Page = PageNumber, PageSize = PageSize }, ct);
        TotalInvoices = result.TotalCount;

        var items = result.Items?.ToList() ?? new List<InvoiceDto>();

        // Apply client-side filter (search by customer name / payment type)
        // For full server-side filtering, add query params to the API call
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var lower = SearchTerm.ToLower();
            items = items.Where(i =>
                (i.CustomerName?.ToLower().Contains(lower) ?? false) ||
                (i.ProcessedBy?.ToLower().Contains(lower) ?? false)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(PayTypeFilter) &&
            Enum.TryParse<Store.Models.Enums.PaymentType>(PayTypeFilter, out var pt))
        {
            items = items.Where(i => i.PaymentType == pt).ToList();
        }

        Invoices = items;
        return Page();
    }

    /// <summary>Returns invoice detail JSON for the detail modal.</summary>
    public async Task<IActionResult> OnGetDetailAsync(Guid id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return Unauthorized();
        var invoice = await _invoiceService.GetByIdAsync(id, ct);
        if (invoice is null) return NotFound();
        return new JsonResult(invoice, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    public async Task<IActionResult> OnPostVoidAsync(Guid invoiceId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        try
        {
            var success = await _invoiceService.VoidInvoiceAsync(invoiceId, null, ct);
            StatusMessage = success ? "Invoice voided." : "Error: Invoice not found or already voided.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddTenderAsync([FromQuery] Guid invoiceId, [FromBody] AddTenderRequest request, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(new { message = "Invalid request." });

        try
        {
            var tender = await _invoiceService.AddTenderAsync(invoiceId, request, ct);
            return new JsonResult(tender, new System.Text.Json.JsonSerializerOptions
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
}
