using Store.Models.DTOs.Common;
using Store.Models.DTOs.Transfers;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiStockTransferService : IStockTransferService
{
    private readonly IApiClientService _client;

    public ApiStockTransferService(IApiClientService client) => _client = client;

    public async Task<TransferMetricsDto> GetTransferMetricsAsync(CancellationToken ct = default)
    {
        var result = await _client.GetAsync<TransferMetricsDto>("/api/stocktransfers/metrics", ct);
        return result ?? new TransferMetricsDto();
    }

    public async Task<PagedResult<StockTransferDto>> GetTransfersPagedAsync(TransferFilterRequest request, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (request.BranchId.HasValue) qs.Add($"branchId={request.BranchId.Value}");
        if (!string.IsNullOrWhiteSpace(request.Status)) qs.Add($"status={Uri.EscapeDataString(request.Status)}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm)) qs.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (request.FromDate.HasValue) qs.Add($"fromDate={Uri.EscapeDataString(request.FromDate.Value.ToString("O"))}");
        if (request.ToDate.HasValue) qs.Add($"toDate={Uri.EscapeDataString(request.ToDate.Value.ToString("O"))}");

        var query = "?" + string.Join("&", qs);
        var result = await _client.GetAsync<PagedResult<StockTransferDto>>($"/api/stocktransfers/paged{query}", ct);
        return result ?? new PagedResult<StockTransferDto>();
    }

    public async Task<List<StockTransferDto>> GetAllAsync(int? branchId = null, string? status = null)
    {
        var qs = new List<string>();
        if (branchId.HasValue) qs.Add($"branchId={branchId.Value}");
        if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={Uri.EscapeDataString(status)}");
        var query = qs.Count > 0 ? "?" + string.Join("&", qs) : "";
        var result = await _client.GetAsync<List<StockTransferDto>>($"/api/stocktransfers{query}");
        return result ?? new List<StockTransferDto>();
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id)
        => await _client.GetAsync<StockTransferDto>($"/api/stocktransfers/{id}");

    public async Task<StockTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedByUserId)
    {
        var result = await _client.PostAsync<StockTransferDto>("/api/stocktransfers", request);
        return result ?? throw new InvalidOperationException("Failed to create transfer.");
    }

    public async Task<StockTransferDto?> ApproveAsync(int id, Guid approvedByUserId, ApproveTransferRequest request)
        => await _client.PostAsync<StockTransferDto>($"/api/stocktransfers/{id}/approve", request);

    public async Task<bool> RejectAsync(int id, Guid userId, RejectTransferRequest request)
        => await _client.PostAsync($"/api/stocktransfers/{id}/reject", request);

    public async Task<StockTransferDto?> DispatchAsync(int id, Guid dispatchedByUserId, DispatchTransferRequest request)
        => await _client.PostAsync<StockTransferDto>($"/api/stocktransfers/{id}/dispatch", request);

    public async Task<StockTransferDto?> ReceiveAsync(int id, Guid receivedByUserId, ReceiveTransferRequest request)
        => await _client.PostAsync<StockTransferDto>($"/api/stocktransfers/{id}/receive", request);

    public async Task<bool> CancelAsync(int id, Guid userId, string? reason)
        => await _client.PostAsync($"/api/stocktransfers/{id}/cancel", reason);
}
