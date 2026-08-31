using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

[AllowAnonymous]
public class ReceiptModel : PageModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<ReceiptModel> _logger;

    public ReceiptModel(
        IInvoiceService invoiceService,
        IApiClientService apiClient,
        ILogger<ReceiptModel> logger)
    {
        _invoiceService = invoiceService;
        _apiClient = apiClient;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    public PublicReceiptDto? Receipt { get; set; }
    public bool NotFoundState { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!Id.HasValue || Id.Value == Guid.Empty)
        {
            NotFoundState = true;
            return Page();
        }

        try
        {
            // Direct service resolution
            Receipt = await _invoiceService.GetPublicReceiptAsync(Id.Value, ct);

            // Fallback via API client if running in decoupled mode
            if (Receipt == null)
            {
                var resp = await _apiClient.GetAsync<ApiResponse<PublicReceiptDto>>($"/api/invoices/public/{Id.Value}");
                Receipt = resp?.Data;
            }

            if (Receipt == null)
            {
                NotFoundState = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load public receipt {ReceiptId}", Id);
            NotFoundState = true;
        }

        return Page();
    }
}
