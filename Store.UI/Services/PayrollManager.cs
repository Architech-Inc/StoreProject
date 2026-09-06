using Store.Models.Entities.HR;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.HR;
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

    public async Task<IEnumerable<TaxBracketDto>> GetTaxBracketsAsync(CancellationToken ct = default)
    {
        var response = await _apiClient.GetAsync<ApiResponse<IEnumerable<TaxBracketDto>>>("api/taxbrackets", ct);
        return response?.Data ?? Enumerable.Empty<TaxBracketDto>();
    }

    public async Task<ApiResponse<TaxBracketDto>> SaveTaxBracketAsync(int id, decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive, CancellationToken ct = default)
    {
        var payload = new { minAmount, maxAmount, taxPercentage, fixedTaxAmount, isActive };
        if (id == 0)
        {
            return await _apiClient.PostAsync<ApiResponse<TaxBracketDto>>("api/taxbrackets", payload, ct)
                ?? ApiResponse<TaxBracketDto>.Fail("Failed to create tax bracket.");
        }
        else
        {
            return await _apiClient.PutAsync<ApiResponse<TaxBracketDto>>($"api/taxbrackets/{id}", payload, ct)
                ?? ApiResponse<TaxBracketDto>.Fail("Failed to update tax bracket.");
        }
    }

    public async Task<bool> DeleteTaxBracketAsync(int id, CancellationToken ct = default)
    {
        return await _apiClient.DeleteAsync($"api/taxbrackets/{id}", ct);
    }
}
