using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.DTOs.Invoices;
using Store.Models.DTOs.Items;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PosModel : PageModel
{
    private readonly IItemService _itemService;
    private readonly ICustomerService _customerService;
    private readonly IInvoiceService _invoiceService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<PosModel> _logger;

    public IReadOnlyList<ItemDto> CatalogItems { get; private set; } = Array.Empty<ItemDto>();
    public IReadOnlyList<CustomerDto> Customers { get; private set; } = Array.Empty<CustomerDto>();

    public bool HasLoadError { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public PosModel(
        IItemService itemService,
        ICustomerService customerService,
        IInvoiceService invoiceService,
        IApiClientService apiClient,
        ILogger<PosModel> logger)
    {
        _itemService = itemService;
        _customerService = customerService;
        _invoiceService = invoiceService;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("Login");
        }

        _apiClient.SetToken(token);

        try
        {
            var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 500 }, ct);
            var customersTask = _customerService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 200 }, ct);

            await Task.WhenAll(itemsTask, customersTask);

            var items = await itemsTask;
            var customers = await customersTask;

            CatalogItems = items.Items
                .Where(i => i.IsActive && i.InStock > 0)
                .OrderBy(i => i.Name)
                .ToList();

            Customers = customers.Items
                .OrderBy(c => c.FullName)
                .ToList();
        }
        catch (Exception ex)
        {
            HasLoadError = true;
            LoadErrorMessage = "POS data could not be loaded from API.";
            _logger.LogError(ex, "POS initial load failed");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCheckoutAsync([FromBody] PosCheckoutRequest request, CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return new JsonResult(new { success = false, message = "Session expired. Please login again." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        _apiClient.SetToken(token);

        if (request.Lines.Count == 0)
        {
            return BadRequest(new { success = false, message = "At least one sale line is required." });
        }

        if (request.AmountTendered < 0)
        {
            return BadRequest(new { success = false, message = "Amount tendered cannot be negative." });
        }

        try
        {
            var create = new CreateInvoiceRequest
            {
                CustomerId = request.CustomerId,
                PaymentType = request.PaymentType,
                AmountTendered = request.AmountTendered,
                Notes = request.Notes,
                Lines = request.Lines.Select(l => new CreateSaleLineRequest
                {
                    ItemId = l.ItemId,
                    Quantity = l.Quantity
                }).ToList()
            };

            var created = await _invoiceService.CreateInvoiceAsync(create, null, ct);

            return new JsonResult(new
            {
                success = true,
                message = "Checkout completed.",
                invoiceId = created.InvoiceId,
                totalAmount = created.TotalAmount,
                amountTendered = created.AmountTendered,
                changeGiven = created.ChangeGiven,
                paymentType = created.PaymentType.ToString(),
                createdAt = created.DateCreated
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "POS checkout failed due to business rule");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POS checkout failed unexpectedly");
            return StatusCode(500, new { success = false, message = "Unexpected error during checkout." });
        }
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Remove("access_token");
        HttpContext.Session.Remove("refresh_token");
        return RedirectToPage("Login");
    }

    public sealed class PosCheckoutRequest
    {
        public Guid? CustomerId { get; set; }
        public PaymentType PaymentType { get; set; } = PaymentType.Cash;
        public decimal AmountTendered { get; set; }
        public string? Notes { get; set; }
        public List<PosCheckoutLine> Lines { get; set; } = new();
    }

    public sealed class PosCheckoutLine
    {
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
