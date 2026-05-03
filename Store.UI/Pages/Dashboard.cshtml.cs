using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class DashboardModel : SecurePageModel
{
    private readonly IItemService _itemService;
    private readonly IInvoiceService _invoiceService;
    private readonly ICustomerService _customerService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<DashboardModel> _logger;

    public int TotalItems { get; private set; }
    public int TotalCustomers { get; private set; }
    public int TotalInvoices { get; private set; }
    public int LowStockItems { get; private set; }

    public IReadOnlyList<InvoiceDto> RecentInvoices { get; private set; } = Array.Empty<InvoiceDto>();
    public IReadOnlyList<ItemDto> LowStockList { get; private set; } = Array.Empty<ItemDto>();

    public bool HasLoadError { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public bool CanInventoryRead { get; private set; }
    public bool CanPricingRead { get; private set; }
    public bool CanCashRead { get; private set; }
    public bool CanReportsRead { get; private set; }
    public bool CanAdminRoleMatrix { get; private set; }
    public bool CanPaymentsRead { get; private set; }

    public DashboardModel(
        IItemService itemService,
        IInvoiceService invoiceService,
        ICustomerService customerService,
        IApiClientService apiClient,
        ILogger<DashboardModel> logger)
    {
        _itemService = itemService;
        _invoiceService = invoiceService;
        _customerService = customerService;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        CanInventoryRead = HasPermission(permissions, PermissionKeys.InventoryRead);
        CanPricingRead = HasPermission(permissions, PermissionKeys.PricingRead);
        CanCashRead = HasPermission(permissions, PermissionKeys.CashRead);
        CanReportsRead = HasPermission(permissions, PermissionKeys.ReportsRead);
        CanAdminRoleMatrix = HasPermission(permissions, PermissionKeys.AdminRoleMatrix);
        CanPaymentsRead = HasPermission(permissions, PermissionKeys.PaymentsRead);

        try
        {
            var summaryRequest = new PagedRequest { Page = 1, PageSize = 1 };
            var invoiceRequest = new PagedRequest { Page = 1, PageSize = 8 };

            var itemTask = _itemService.GetAllAsync(summaryRequest, ct);
            var customerTask = _customerService.GetAllAsync(summaryRequest, ct);
            var invoiceTask = _invoiceService.GetAllAsync(invoiceRequest, ct);
            var lowStockTask = _itemService.GetLowStockAsync(ct);

            await Task.WhenAll(itemTask, customerTask, invoiceTask, lowStockTask);

            var items = await itemTask;
            var customers = await customerTask;
            var invoices = await invoiceTask;
            var lowStock = (await lowStockTask).ToList();

            TotalItems = items.TotalCount;
            TotalCustomers = customers.TotalCount;
            TotalInvoices = invoices.TotalCount;
            LowStockItems = lowStock.Count;

            RecentInvoices = invoices.Items
                .OrderByDescending(i => i.DateCreated)
                .Take(6)
                .ToList();

            LowStockList = lowStock
                .OrderBy(i => i.InStock)
                .Take(8)
                .ToList();
        }
        catch (Exception ex)
        {
            HasLoadError = true;
            LoadErrorMessage = "Unable to load live dashboard data. Try again.";
            _logger.LogError(ex, "Dashboard data loading failed");
        }

        return Page();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Remove("access_token");
        HttpContext.Session.Remove("refresh_token");
        return RedirectToPage("Login");
    }
}
