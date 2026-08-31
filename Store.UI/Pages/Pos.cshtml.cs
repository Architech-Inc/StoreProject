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

    [BindProperty(SupportsGet = true)]
    public Guid? CustomerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ItemId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? AddItem { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Barcode { get; set; }

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
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("force_reset_userId")))
            {
                return RedirectToPage("/ForceResetPassword", new { returnUrl = "/Pos" });
            }
            return RedirectToPage("/Login");
        }

        if (HttpContext.Session.GetString("force_password_reset") == "true")
        {
            return RedirectToPage("/ForceResetPassword", new { returnUrl = "/Pos" });
        }

        _apiClient.SetToken(token);

        try
        {
            var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 500 }, ct);
            var customersTask = _customerService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 200 }, ct);

            await Task.WhenAll(itemsTask, customersTask);

            var items = await itemsTask;
            var customers = await customersTask;

            var catalogList = items.Items
                .Where(i => i.IsActive && i.InStock > 0)
                .OrderBy(i => i.Name)
                .ToList();

            var customerList = customers.Items
                .OrderBy(c => c.FullName)
                .ToList();

            // Ensure targeted customer is present in list if specified by URL
            if (CustomerId.HasValue && !customerList.Any(c => c.CustomerId == CustomerId.Value))
            {
                var directCustomer = await _customerService.GetByIdAsync(CustomerId.Value, ct);
                if (directCustomer != null)
                {
                    customerList.Insert(0, directCustomer);
                }
            }

            // Ensure targeted item is present in list if specified by URL
            var targetItemId = ItemId ?? AddItem;
            if (targetItemId.HasValue && !catalogList.Any(i => i.ItemId == targetItemId.Value))
            {
                var directItem = await _itemService.GetByIdAsync(targetItemId.Value, ct);
                if (directItem != null && directItem.IsActive && directItem.InStock > 0)
                {
                    catalogList.Insert(0, directItem);
                }
            }

            CatalogItems = catalogList;
            Customers = customerList;
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

    public async Task<IActionResult> OnGetCatalogDataAsync(CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        _apiClient.SetToken(token);

        try
        {
            var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 1000 }, ct);
            var customersTask = _customerService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 500 }, ct);

            await Task.WhenAll(itemsTask, customersTask);

            var items = (await itemsTask).Items
                .Where(i => i.IsActive && i.InStock > 0)
                .Select(i => new
                {
                    itemId = i.ItemId,
                    name = i.Name,
                    barcode = i.Barcode,
                    unitPrice = i.UnitPrice,
                    inStock = i.InStock,
                    categoryName = i.CategoryName,
                    unit = i.UnitAbbreviation,
                    discountPercentage = i.DiscountPercentage
                })
                .ToList();

            var customers = (await customersTask).Items
                .Select(c => new
                {
                    customerId = c.CustomerId,
                    fullName = c.FullName,
                    primaryPhone = c.PrimaryPhone
                })
                .ToList();

            return new JsonResult(new { success = true, items, customers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch catalog data for offline sync");
            return StatusCode(500, new { success = false, message = "Could not fetch catalog data." });
        }
    }

    public async Task<IActionResult> OnPostOfflineBatchSyncAsync([FromBody] List<PosOfflineBatchEntry> batch, CancellationToken ct)
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

        if (batch == null || batch.Count == 0)
        {
            return BadRequest(new { success = false, message = "Batch is empty." });
        }

        var results = new List<PosSyncResultItem>();
        int synced = 0;
        int failed = 0;

        foreach (var entry in batch)
        {
            try
            {
                var create = new CreateInvoiceRequest
                {
                    CustomerId = entry.Payload.CustomerId,
                    PaymentType = entry.Payload.PaymentType,
                    AmountTendered = entry.Payload.AmountTendered,
                    Notes = $"[Offline Sync: {entry.OfflineReceiptNumber}] {entry.Payload.Notes}".Trim(),
                    Lines = entry.Payload.Lines.Select(l => new CreateSaleLineRequest
                    {
                        ItemId = l.ItemId,
                        Quantity = l.Quantity
                    }).ToList()
                };

                var created = await _invoiceService.CreateInvoiceAsync(create, null, ct);
                synced++;
                results.Add(new PosSyncResultItem
                {
                    ClientTxId = entry.ClientTxId,
                    OfflineReceiptNumber = entry.OfflineReceiptNumber,
                    Success = true,
                    ServerInvoiceId = created.InvoiceId,
                    TotalAmount = created.TotalAmount,
                    ChangeGiven = created.ChangeGiven
                });
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning(ex, "Failed to sync offline transaction {ClientTxId} ({Receipt})", entry.ClientTxId, entry.OfflineReceiptNumber);
                results.Add(new PosSyncResultItem
                {
                    ClientTxId = entry.ClientTxId,
                    OfflineReceiptNumber = entry.OfflineReceiptNumber,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return new JsonResult(new
        {
            success = true,
            syncedCount = synced,
            failedCount = failed,
            results
        });
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Remove("access_token");
        HttpContext.Session.Remove("refresh_token");
        return RedirectToPage("/Login");
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

    public sealed class PosOfflineBatchEntry
    {
        public string ClientTxId { get; set; } = string.Empty;
        public string OfflineReceiptNumber { get; set; } = string.Empty;
        public PosCheckoutRequest Payload { get; set; } = new();
    }

    public sealed class PosSyncResultItem
    {
        public string ClientTxId { get; set; } = string.Empty;
        public string OfflineReceiptNumber { get; set; } = string.Empty;
        public bool Success { get; set; }
        public Guid? ServerInvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ChangeGiven { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
