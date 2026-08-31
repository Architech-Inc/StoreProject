using Store.Models.DTOs.Cash;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiCashVarianceService : ICashVarianceService
{
    private readonly IApiClientService _client;

    public ApiCashVarianceService(IApiClientService client) => _client = client;

    public async Task<CashVarianceMetricsDto> GetMetricsAsync()
    {
        return await _client.GetAsync<CashVarianceMetricsDto>("/api/cash/variances/metrics")
            ?? new CashVarianceMetricsDto();
    }

    public async Task<List<CashVarianceDto>> GetAllAsync(CashVarianceStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var qParams = new List<string>();
        if (status.HasValue) qParams.Add($"status={status.Value}");
        if (fromDate.HasValue) qParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
        if (toDate.HasValue) qParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");

        var query = qParams.Count > 0 ? "?" + string.Join("&", qParams) : "";
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
