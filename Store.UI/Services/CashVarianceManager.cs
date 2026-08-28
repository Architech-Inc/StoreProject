using System.Text;
using Store.Models.DTOs.Cash;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;

namespace StoreUI.Services;

public class CashVarianceManager : ICashVarianceManager
{
    private readonly IApiClientService _apiClient;

    public CashVarianceManager(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<CashVarianceMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<CashVarianceMetricsDto>("/api/cash/variances/metrics", ct)
            ?? new CashVarianceMetricsDto();
    }

    public async Task<List<CashVarianceDto>> GetAllAsync(CashVarianceStatus? status = null, CancellationToken ct = default)
    {
        var query = status.HasValue ? $"?status={status.Value}" : "";
        return await _apiClient.GetAsync<List<CashVarianceDto>>($"/api/cash/variances{query}", ct)
            ?? new List<CashVarianceDto>();
    }

    public async Task<CashVarianceDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<CashVarianceDto>($"/api/cash/variances/{id}", ct);
    }

    public async Task<List<CashVarianceDto>> GetByShiftAsync(Guid shiftId, CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<List<CashVarianceDto>>($"/api/cash/variances/by-shift/{shiftId}", ct)
            ?? new List<CashVarianceDto>();
    }

    public async Task<List<CashierShiftDto>> SearchShiftsAsync(string? query = null, int limit = 30, CancellationToken ct = default)
    {
        var shifts = await _apiClient.GetAsync<List<CashierShiftDto>>($"/api/cash/shifts?page=1&pageSize={limit}", ct) ?? new();
        if (string.IsNullOrWhiteSpace(query))
            return shifts;

        var q = query.Trim().ToLowerInvariant();
        return shifts.Where(s =>
            s.CashierShiftId.ToString().ToLowerInvariant().Contains(q) ||
            s.Status.ToString().ToLowerInvariant().Contains(q) ||
            (s.Notes?.ToLowerInvariant().Contains(q) ?? false)
        ).ToList();
    }

    public async Task<CashVarianceDto?> RecordAsync(RecordCashVarianceRequest request, CancellationToken ct = default)
    {
        return await _apiClient.PostAsync<CashVarianceDto>("/api/cash/variances", request, ct);
    }

    public async Task<CashVarianceDto?> ReviewAsync(int id, ReviewCashVarianceRequest request, CancellationToken ct = default)
    {
        return await _apiClient.PostAsync<CashVarianceDto>($"/api/cash/variances/{id}/review", request, ct);
    }

    public byte[] GenerateCsv(List<CashVarianceDto> list, CashVarianceMetricsDto metrics)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# ClexAn Foods - Cash Variance & Float Audit Report ({DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC)");
        sb.AppendLine($"# Total Audits: {metrics.TotalRecords} | Pending: {metrics.TotalPendingCount} | Reviewed: {metrics.TotalReviewedCount} | Escalated: {metrics.TotalEscalatedCount}");
        sb.AppendLine($"# Net Discrepancy (XAF): {metrics.NetDiscrepancyXaf:N0} | Total Shortages: -{metrics.TotalShortagesXaf:N0} XAF | Total Overages: +{metrics.TotalOveragesXaf:N0} XAF");
        sb.AppendLine();
        sb.AppendLine("Record ID,Shift ID,Expected Amount (XAF),Actual Counted (XAF),Variance (XAF),Discrepancy Type,Reason Code,Status,Cashier User,Supervisor Reviewer,Review Notes,Reviewed At,Date Recorded");

        foreach (var v in list)
        {
            var discType = v.IsShortage ? "SHORTAGE" : v.IsOverage ? "OVERAGE" : "EXACT_MATCH";
            sb.AppendLine($"{v.CashVarianceRecordId},\"{v.CashierShiftId}\",{v.ExpectedAmount:N0},{v.ActualAmount:N0},{v.Variance:N0},{discType},\"{v.ReasonCode ?? "—"}\",{v.Status},\"{v.RecordedByUser}\",\"{v.ReviewedByUser ?? "—"}\",\"{v.ReviewNotes?.Replace("\"", "\"\"") ?? ""}\",\"{v.ReviewedAt:yyyy-MM-dd HH:mm}\",\"{v.DateCreated:yyyy-MM-dd HH:mm}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
