using Store.Models.DTOs.Cash;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiCashVarianceService : ICashVarianceService
{
    private readonly IApiClientService _client;

    public ApiCashVarianceService(IApiClientService client) => _client = client;

    public async Task<List<CashVarianceDto>> GetAllAsync(CashVarianceStatus? status = null)
    {
        var query = status.HasValue ? $"?status={status.Value}" : "";
        return await _client.GetAsync<List<CashVarianceDto>>($"/api/cash/variances{query}") ?? new();
    }

    public async Task<CashVarianceDto?> GetByIdAsync(int id)
        => await _client.GetAsync<CashVarianceDto>($"/api/cash/variances/{id}");

    public async Task<List<CashVarianceDto>> GetByShiftAsync(Guid cashierShiftId)
        => await _client.GetAsync<List<CashVarianceDto>>($"/api/cash/variances/by-shift/{cashierShiftId}") ?? new();

    public async Task<CashVarianceDto> RecordAsync(RecordCashVarianceRequest request, Guid recordedByUserId)
    {
        var result = await _client.PostAsync<CashVarianceDto>("/api/cash/variances", request);
        return result ?? throw new InvalidOperationException("Failed to record cash variance.");
    }

    public async Task<CashVarianceDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewCashVarianceRequest request)
        => await _client.PostAsync<CashVarianceDto>($"/api/cash/variances/{id}/review", request);
}
