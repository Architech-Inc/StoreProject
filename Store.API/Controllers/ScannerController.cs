using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Scanner;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScannerController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly IInvoiceService _invoiceService;
    private readonly IEmployeeService _employeeService;
    private readonly ISupplierService _supplierService;
    private readonly IBatchService _batchService;
    private readonly ILogger<ScannerController> _logger;

    public ScannerController(
        IItemService itemService,
        IInvoiceService invoiceService,
        IEmployeeService employeeService,
        ISupplierService supplierService,
        IBatchService batchService,
        ILogger<ScannerController> logger)
    {
        _itemService = itemService;
        _invoiceService = invoiceService;
        _employeeService = employeeService;
        _supplierService = supplierService;
        _batchService = batchService;
        _logger = logger;
    }

    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(ApiResponse<object>.Fail("Scan code cannot be empty."));
        }

        var trimmedCode = code.Trim();
        _logger.LogInformation("Resolving scanned code: {Code}", trimmedCode);

        // 1. Check Items (by Barcode or ID)
        var itemsResult = await _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 10, SearchTerm = trimmedCode }, ct);
        var matchedItem = itemsResult.Items?.FirstOrDefault(i =>
            string.Equals(i.Barcode, trimmedCode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(i.Name, trimmedCode, StringComparison.OrdinalIgnoreCase))
            ?? (Guid.TryParse(trimmedCode, out var itemGuid) ? await _itemService.GetByIdAsync(itemGuid, ct) : null)
            ?? itemsResult.Items?.FirstOrDefault();

        if (matchedItem != null)
        {
            var result = new ScanResolutionResultDto
            {
                EntityType = ScanEntityTypes.Item,
                Code = trimmedCode,
                Title = matchedItem.Name,
                Subtitle = $"{matchedItem.CategoryName ?? "General"} • {matchedItem.UnitAbbreviation ?? "Unit"}",
                ThumbnailUrl = matchedItem.ThumbnailUrl ?? matchedItem.FullImageUrl,
                EntityId = matchedItem.ItemId.ToString(),
                Details = new Dictionary<string, string>
                {
                    ["Price"] = $"{matchedItem.UnitPrice:N0} XAF",
                    ["Stock"] = $"{matchedItem.InStock} {matchedItem.UnitAbbreviation ?? "units"}",
                    ["Barcode"] = matchedItem.Barcode ?? trimmedCode,
                    ["Status"] = matchedItem.InStock <= 0 ? "Out of Stock" : (matchedItem.InStock <= (matchedItem.ReorderLevel ?? 5) ? "Low Stock" : "In Stock")
                },
                Actions = new List<ScanActionDto>
                {
                    new()
                    {
                        ActionId = "pos_add",
                        Label = "Sell in POS Terminal",
                        Icon = "cart",
                        TargetUrl = $"/Pos?addItem={matchedItem.ItemId}&barcode={Uri.EscapeDataString(matchedItem.Barcode ?? trimmedCode)}",
                        ButtonClass = "button-primary",
                        ShortcutKey = "1"
                    },
                    new()
                    {
                        ActionId = "catalog_edit",
                        Label = "Edit in Catalog",
                        Icon = "edit",
                        TargetUrl = $"/Catalog?edit={matchedItem.ItemId}",
                        ButtonClass = "button-command",
                        ShortcutKey = "2"
                    },
                    new()
                    {
                        ActionId = "log_wastage",
                        Label = "Record Wastage",
                        Icon = "trash",
                        TargetUrl = $"/Wastage?itemId={matchedItem.ItemId}",
                        ButtonClass = "button-command",
                        ShortcutKey = "3"
                    },
                    new()
                    {
                        ActionId = "stock_transfer",
                        Label = "Transfer to Branch",
                        Icon = "swap",
                        TargetUrl = $"/StockTransfers?itemId={matchedItem.ItemId}",
                        ButtonClass = "button-command",
                        ShortcutKey = "4"
                    },
                    new()
                    {
                        ActionId = "price_adjust",
                        Label = "Adjust Price / Discount",
                        Icon = "tag",
                        TargetUrl = $"/PricingOps?itemId={matchedItem.ItemId}",
                        ButtonClass = "button-command",
                        ShortcutKey = "5"
                    },
                    new()
                    {
                        ActionId = "batch_track",
                        Label = "Inspect Batches & Expiry",
                        Icon = "box",
                        TargetUrl = $"/BatchTracking?barcode={Uri.EscapeDataString(matchedItem.Barcode ?? trimmedCode)}",
                        ButtonClass = "button-command",
                        ShortcutKey = "6"
                    }
                }
            };
            return Ok(ApiResponse<ScanResolutionResultDto>.Ok(result));
        }

        // 2. Check Invoices (by InvoiceId)
        var invoicesResult = await _invoiceService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 10, SearchTerm = trimmedCode }, ct);
        var matchedInvoice = (Guid.TryParse(trimmedCode, out var invGuid) ? await _invoiceService.GetByIdAsync(invGuid, ct) : null)
            ?? invoicesResult.Items?.FirstOrDefault(i => i.InvoiceId.ToString().Contains(trimmedCode, StringComparison.OrdinalIgnoreCase))
            ?? invoicesResult.Items?.FirstOrDefault();

        if (matchedInvoice != null)
        {
            var shortId = matchedInvoice.InvoiceId.ToString().Length >= 8 ? matchedInvoice.InvoiceId.ToString()[..8].ToUpper() : matchedInvoice.InvoiceId.ToString();
            var result = new ScanResolutionResultDto
            {
                EntityType = ScanEntityTypes.Invoice,
                Code = trimmedCode,
                Title = $"Invoice #{shortId}",
                Subtitle = $"Total: {matchedInvoice.TotalAmount:N0} XAF • {matchedInvoice.DateCreated:MMM dd, yyyy HH:mm}",
                EntityId = matchedInvoice.InvoiceId.ToString(),
                Details = new Dictionary<string, string>
                {
                    ["Status"] = matchedInvoice.IsPaid ? "Paid" : "Pending",
                    ["Customer"] = matchedInvoice.CustomerName ?? "Walk-in Customer",
                    ["Lines Count"] = matchedInvoice.Lines?.Count().ToString() ?? "N/A",
                    ["Total Amount"] = $"{matchedInvoice.TotalAmount:N0} XAF"
                },
                Actions = new List<ScanActionDto>
                {
                    new()
                    {
                        ActionId = "view_invoice",
                        Label = "View / Print Receipt",
                        Icon = "file-text",
                        TargetUrl = $"/Invoices?id={matchedInvoice.InvoiceId}",
                        ButtonClass = "button-primary",
                        ShortcutKey = "1"
                    },
                    new()
                    {
                        ActionId = "refund_invoice",
                        Label = "Process Return / Refund",
                        Icon = "rotate-ccw",
                        TargetUrl = $"/Invoices?id={matchedInvoice.InvoiceId}&action=refund",
                        ButtonClass = "button-command",
                        ShortcutKey = "2"
                    }
                }
            };
            return Ok(ApiResponse<ScanResolutionResultDto>.Ok(result));
        }

        // 3. Check Employees / Users (by NID, Name, or ID)
        var employeesResult = await _employeeService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 10, SearchTerm = trimmedCode }, ct);
        var matchedEmployee = employeesResult.Items?.FirstOrDefault(e =>
            string.Equals(e.NidNumber, trimmedCode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.FullName, trimmedCode, StringComparison.OrdinalIgnoreCase))
            ?? (Guid.TryParse(trimmedCode, out var empGuid) ? await _employeeService.GetByIdAsync(empGuid, ct) : null);

        if (matchedEmployee != null)
        {
            var result = new ScanResolutionResultDto
            {
                EntityType = ScanEntityTypes.User,
                Code = trimmedCode,
                Title = matchedEmployee.FullName,
                Subtitle = $"{matchedEmployee.DepartmentName ?? "Staff"} • Status: {matchedEmployee.Status}",
                ThumbnailUrl = matchedEmployee.ThumbnailUrl ?? matchedEmployee.FullImageUrl,
                EntityId = matchedEmployee.EmployeeId.ToString(),
                Details = new Dictionary<string, string>
                {
                    ["NID / ID"] = matchedEmployee.NidNumber ?? "N/A",
                    ["Status"] = matchedEmployee.Status.ToString(),
                    ["Department"] = matchedEmployee.DepartmentName ?? "General",
                    ["Employed Since"] = matchedEmployee.DateEmployed.ToString("MMM yyyy")
                },
                Actions = new List<ScanActionDto>
                {
                    new()
                    {
                        ActionId = "view_employee",
                        Label = "View Staff Profile & Shifts",
                        Icon = "user",
                        TargetUrl = $"/BranchAdmin?employeeId={matchedEmployee.EmployeeId}",
                        ButtonClass = "button-primary",
                        ShortcutKey = "1"
                    }
                }
            };
            return Ok(ApiResponse<ScanResolutionResultDto>.Ok(result));
        }

        // 4. Check Suppliers (by RegistrationNumber or Name)
        var suppliers = await _supplierService.GetAllAsync();
        var matchedSupplier = suppliers?.FirstOrDefault(s =>
            string.Equals(s.RegistrationNumber, trimmedCode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s.Name, trimmedCode, StringComparison.OrdinalIgnoreCase));

        if (matchedSupplier != null)
        {
            var result = new ScanResolutionResultDto
            {
                EntityType = ScanEntityTypes.Supplier,
                Code = trimmedCode,
                Title = matchedSupplier.Name,
                Subtitle = $"Reg: {matchedSupplier.RegistrationNumber ?? "N/A"}",
                EntityId = matchedSupplier.SupplierId.ToString(),
                Details = new Dictionary<string, string>
                {
                    ["Registration"] = matchedSupplier.RegistrationNumber ?? "N/A",
                    ["Phones"] = matchedSupplier.Phones != null && matchedSupplier.Phones.Any() ? string.Join(", ", matchedSupplier.Phones) : "N/A",
                    ["Emails"] = matchedSupplier.Emails != null && matchedSupplier.Emails.Any() ? string.Join(", ", matchedSupplier.Emails) : "N/A"
                },
                Actions = new List<ScanActionDto>
                {
                    new()
                    {
                        ActionId = "create_po",
                        Label = "Create Purchase Order",
                        Icon = "truck",
                        TargetUrl = $"/PurchaseOrders?supplierId={matchedSupplier.SupplierId}",
                        ButtonClass = "button-primary",
                        ShortcutKey = "1"
                    },
                    new()
                    {
                        ActionId = "view_supplier",
                        Label = "View Supplier Profile",
                        Icon = "external-link",
                        TargetUrl = $"/Suppliers?id={matchedSupplier.SupplierId}",
                        ButtonClass = "button-command",
                        ShortcutKey = "2"
                    }
                }
            };
            return Ok(ApiResponse<ScanResolutionResultDto>.Ok(result));
        }

        // 5. Check Batches (by BatchNumber)
        var batches = await _batchService.GetAllAsync();
        var matchedBatch = batches?.FirstOrDefault(b =>
            string.Equals(b.BatchNumber, trimmedCode, StringComparison.OrdinalIgnoreCase));

        if (matchedBatch != null)
        {
            var result = new ScanResolutionResultDto
            {
                EntityType = ScanEntityTypes.Batch,
                Code = trimmedCode,
                Title = $"Batch #{matchedBatch.BatchNumber}",
                Subtitle = $"Expires: {matchedBatch.ExpiryDate:MMM dd, yyyy} • Stock: {matchedBatch.Quantity}",
                EntityId = matchedBatch.BatchId.ToString(),
                Details = new Dictionary<string, string>
                {
                    ["Item"] = matchedBatch.ItemName ?? "Product Item",
                    ["Remaining Qty"] = matchedBatch.Quantity.ToString(),
                    ["Expiry Status"] = matchedBatch.ExpiryStatus ?? "Valid",
                    ["Received Date"] = matchedBatch.ReceivedDate.ToString("MMM dd, yyyy")
                },
                Actions = new List<ScanActionDto>
                {
                    new()
                    {
                        ActionId = "view_batch",
                        Label = "Inspect in Batch Tracking",
                        Icon = "box",
                        TargetUrl = $"/BatchTracking?batchId={matchedBatch.BatchId}",
                        ButtonClass = "button-primary",
                        ShortcutKey = "1"
                    }
                }
            };
            return Ok(ApiResponse<ScanResolutionResultDto>.Ok(result));
        }

        // 6. Unknown / Unregistered Code Fallback
        var unknownResult = new ScanResolutionResultDto
        {
            EntityType = ScanEntityTypes.Unknown,
            Code = trimmedCode,
            Title = "Unrecognized Barcode / QR Code",
            Subtitle = $"Scanned value: {trimmedCode}",
            Details = new Dictionary<string, string>
            {
                ["Scanned Code"] = trimmedCode,
                ["Database Status"] = "Not matched with any existing entity"
            },
            Actions = new List<ScanActionDto>
            {
                new()
                {
                    ActionId = "register_product",
                    Label = "Register as New Product in Catalog",
                    Icon = "plus-circle",
                    TargetUrl = $"/Catalog?newBarcode={Uri.EscapeDataString(trimmedCode)}",
                    ButtonClass = "button-primary",
                    ShortcutKey = "1"
                },
                new()
                {
                    ActionId = "search_catalog",
                    Label = "Search Entire Catalog",
                    Icon = "search",
                    TargetUrl = $"/Catalog?search={Uri.EscapeDataString(trimmedCode)}",
                    ButtonClass = "button-command",
                    ShortcutKey = "2"
                }
            }
        };

        return Ok(ApiResponse<ScanResolutionResultDto>.Ok(unknownResult));
    }
}
