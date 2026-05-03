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
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        var result = await _client.GetAsync<PagedResult<InvoiceDto>>($"/api/invoices{qs}", ct);
        return result ?? new PagedResult<InvoiceDto>();
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<InvoiceDto>("/api/invoices", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create invoice");
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<bool?>($"/api/invoices/{invoiceId}/void", new { }, ct);
        return result.HasValue && result.Value;
    }

    public async Task<InvoiceTenderDto> AddTenderAsync(Guid invoiceId, AddTenderRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<InvoiceTenderDto>($"/api/invoices/{invoiceId}/tender", request, ct);
        return result ?? throw new InvalidOperationException("Failed to record tender.");
    }
}
