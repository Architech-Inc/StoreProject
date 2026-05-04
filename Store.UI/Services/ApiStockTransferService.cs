using Store.Models.DTOs.Transfers;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiStockTransferService : IStockTransferService
{
    private readonly IApiClientService _client;

    public ApiStockTransferService(IApiClientService client) => _client = client;

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
