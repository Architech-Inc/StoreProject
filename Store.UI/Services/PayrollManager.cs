using Store.Models.Entities.HR;
using Store.Models.DTOs.Common;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Services;

public class PayrollManager : IPayrollManager
{
    private readonly IApiClientService _apiClient;

    public PayrollManager(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<PayrollRun>> GetAllRunsAsync()
    {
        var response = await _apiClient.GetAsync<ApiResponse<IEnumerable<PayrollRun>>>("api/payroll");
        return response?.Data ?? Enumerable.Empty<PayrollRun>();
    }

    public async Task<PayrollRun?> GetRunByIdAsync(Guid id)
    {
        var response = await _apiClient.GetAsync<ApiResponse<PayrollRun>>($"api/payroll/{id}");
        return response?.Data;
    }

    public async Task<IEnumerable<Payslip>> GetPayslipsAsync(Guid runId)
    {
        var response = await _apiClient.GetAsync<ApiResponse<IEnumerable<Payslip>>>($"api/payroll/{runId}/payslips");
        return response?.Data ?? Enumerable.Empty<Payslip>();
    }

    public async Task<ApiResponse<PayrollRun>> DraftPayrollAsync(DateTime periodStart, DateTime periodEnd)
    {
        var request = new { PeriodStart = periodStart, PeriodEnd = periodEnd };
        return await _apiClient.PostAsync<ApiResponse<PayrollRun>>("api/payroll/draft", request)
            ?? ApiResponse<PayrollRun>.Fail("Failed to draft payroll run.");
    }

    public async Task<ApiResponse<PayrollRun>> ApprovePayrollAsync(Guid runId)
    {
        return await _apiClient.PostAsync<ApiResponse<PayrollRun>>($"api/payroll/{runId}/approve", new { })
            ?? ApiResponse<PayrollRun>.Fail("Failed to approve payroll run.");
    }

    public async Task<ApiResponse> PayPayrollAsync(Guid runId)
    {
        return await _apiClient.PostAsync<ApiResponse>($"api/payroll/{runId}/pay", new { })
            ?? ApiResponse.Fail("Failed to pay payroll run.");
    }
}
