using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiInvoiceService : IInvoiceService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiInvoiceService> _logger;

    public ApiInvoiceService(IApiClientService client, ILogger<ApiInvoiceService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _client.GetAsync<InvoiceDto>($"/api/invoices/{invoiceId}", ct);
    }

    public async Task<PagedResult<InvoiceDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var qs = BuildQueryString(request);
        var result = await _client.GetAsync<PagedResult<InvoiceDto>>($"/api/invoices{qs}", ct);
        return result ?? new PagedResult<InvoiceDto>();
    }

    public async Task<InvoiceSummaryMetricsDto> GetSummaryMetricsAsync(InvoicePagedRequest request, CancellationToken ct = default)
    {
        var qs = BuildQueryString(request);
        var result = await _client.GetAsync<InvoiceSummaryMetricsDto>($"/api/invoices/summary{qs}", ct);
        return result ?? new InvoiceSummaryMetricsDto();
    }

    private static string BuildQueryString(PagedRequest request)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            qs += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";
        }
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            qs += $"&sortBy={Uri.EscapeDataString(request.SortBy)}";
        }
        if (request.SortDescending)
        {
            qs += "&sortDescending=true";
        }

        if (request is InvoicePagedRequest invReq)
        {
            if (invReq.FromDate.HasValue)
                qs += $"&fromDate={invReq.FromDate.Value:yyyy-MM-dd}";
            if (invReq.ToDate.HasValue)
                qs += $"&toDate={invReq.ToDate.Value:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(invReq.Status) && !invReq.Status.Equals("all", StringComparison.OrdinalIgnoreCase))
                qs += $"&status={Uri.EscapeDataString(invReq.Status)}";
            if (invReq.PaymentType.HasValue)
                qs += $"&paymentType={invReq.PaymentType.Value}";
            if (invReq.BranchId.HasValue && invReq.BranchId.Value > 0)
                qs += $"&branchId={invReq.BranchId.Value}";
            if (invReq.MinAmount.HasValue)
                qs += $"&minAmount={invReq.MinAmount.Value}";
            if (invReq.MaxAmount.HasValue)
                qs += $"&maxAmount={invReq.MaxAmount.Value}";
        }

        return qs;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<InvoiceDto>("/api/invoices", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create invoice");
    }

    public Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, CancellationToken ct = default)
    {
        return VoidInvoiceAsync(invoiceId, actingUserId, null, ct);
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, string? reason, CancellationToken ct = default)
    {
        var url = $"/api/invoices/{invoiceId}/void";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            url += $"?reason={Uri.EscapeDataString(reason)}";
        }
        var result = await _client.PostAsync<bool?>(url, new { }, ct);
        return result.HasValue && result.Value;
    }

    public async Task<InvoiceTenderDto> AddTenderAsync(Guid invoiceId, AddTenderRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<InvoiceTenderDto>($"/api/invoices/{invoiceId}/tender", request, ct);
        return result ?? throw new InvalidOperationException("Failed to record tender.");
    }

    public async Task<InvoiceDto?> RefundInvoiceAsync(Guid invoiceId, RefundInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<InvoiceDto>($"/api/invoices/{invoiceId}/refund", request, ct);
        return result ?? throw new InvalidOperationException("Failed to refund invoice.");
    }
}
