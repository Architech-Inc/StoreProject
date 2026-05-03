using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Orders;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class OrdersModel : SecurePageModel
{
    private readonly IOrderService _orderService;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<OrdersModel> _logger;

    public IReadOnlyList<OrderDto> Orders { get; private set; } = Array.Empty<OrderDto>();
    public IReadOnlyList<ItemDto> Items { get; private set; } = Array.Empty<ItemDto>();
    public string? StatusMessage { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int PageSize { get; private set; } = 20;
    public int TotalPages { get; private set; } = 1;

    // Create-order bind properties
    [BindProperty] public string? OrderSupplierName { get; set; }
    [BindProperty] public string? OrderNotes { get; set; }
    // Line items serialized as JSON from client
    [BindProperty] public string? OrderLinesJson { get; set; }

    public OrdersModel(
        IOrderService orderService,
        IItemService itemService,
        IApiClientService apiClient,
        ILogger<OrdersModel> logger)
    {
        _orderService = orderService;
        _itemService = itemService;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        CurrentPage = Math.Max(1, page);

        try
        {
            var ordersTask = _orderService.GetAllAsync(new PagedRequest { Page = CurrentPage, PageSize = PageSize }, ct);
            var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 300 }, ct);

            await Task.WhenAll(ordersTask, itemsTask);

            var result = await ordersTask;
            Orders = (result.Items ?? Array.Empty<OrderDto>()).ToList();
            TotalPages = Math.Max(1, (int)Math.Ceiling((double)(result.TotalCount) / PageSize));

            Items = (await itemsTask).Items
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load orders page");
            StatusMessage = "Error: Could not load orders data.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (string.IsNullOrWhiteSpace(OrderLinesJson))
        {
            StatusMessage = "Error: At least one order line is required.";
            return RedirectToPage();
        }

        List<CreateOrderLineRequest>? lines;
        try
        {
            lines = System.Text.Json.JsonSerializer.Deserialize<List<CreateOrderLineRequest>>(
                OrderLinesJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            StatusMessage = "Error: Invalid order lines data.";
            return RedirectToPage();
        }

        if (lines is null || lines.Count == 0 || lines.Any(l => l.QuantityOrdered <= 0))
        {
            StatusMessage = "Error: Order lines are invalid. Each line needs a positive quantity.";
            return RedirectToPage();
        }

        try
        {
            var request = new CreateOrderRequest
            {
                Notes = string.IsNullOrWhiteSpace(OrderNotes) ? null : OrderNotes.Trim(),
                Lines = lines
            };

            var created = await _orderService.CreateAsync(request, null, ct);
            StatusMessage = $"Purchase order {created.OrderNumber} created successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            StatusMessage = "Error: Could not create purchase order.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReceiveAsync(Guid orderId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var ok = await _orderService.ReceiveOrderAsync(orderId, ct);
            StatusMessage = ok ? "Order marked as received. Stock has been updated." : "Error: Order not found.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive order {OrderId}", orderId);
            StatusMessage = "Error: Could not mark order as received.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid orderId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var ok = await _orderService.CancelOrderAsync(orderId, ct);
            StatusMessage = ok ? "Order cancelled." : "Error: Cannot cancel this order.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order {OrderId}", orderId);
            StatusMessage = "Error: Could not cancel order.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDetailAsync(Guid id, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        try
        {
            var order = await _orderService.GetByIdAsync(id, ct);
            if (order is null)
                return NotFound();

            return new JsonResult(order, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return StatusCode(500);
        }
    }

    public static string StatusBadgeClass(OrderStatus status) => status switch
    {
        OrderStatus.Draft => "badge-draft",
        OrderStatus.Pending => "badge-info",
        OrderStatus.Approved => "badge-warning",
        OrderStatus.PartiallyReceived => "badge-warning",
        OrderStatus.Received => "badge-success",
        OrderStatus.Cancelled => "badge-danger",
        _ => ""
    };
}
