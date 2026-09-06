using Store.Models.Entities.HR;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.HR;

namespace StoreUI.Services;

public interface IPayrollManager
{
    Task<IEnumerable<PayrollRun>> GetAllRunsAsync();
    Task<PayrollRun?> GetRunByIdAsync(Guid id);
    Task<IEnumerable<Payslip>> GetPayslipsAsync(Guid runId);
    Task<ApiResponse<PayrollRun>> DraftPayrollAsync(DateTime periodStart, DateTime periodEnd);
    Task<ApiResponse<PayrollRun>> ApprovePayrollAsync(Guid runId);
    Task<ApiResponse> PayPayrollAsync(Guid runId);

    Task<IEnumerable<TaxBracketDto>> GetTaxBracketsAsync(CancellationToken ct = default);
    Task<ApiResponse<TaxBracketDto>> SaveTaxBracketAsync(int id, decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive, CancellationToken ct = default);
    Task<bool> DeleteTaxBracketAsync(int id, CancellationToken ct = default);
}
