using Store.Models.Entities.HR;
using Store.Models.DTOs.Common;

namespace StoreUI.Services;

public interface IPayrollManager
{
    Task<IEnumerable<PayrollRun>> GetAllRunsAsync();
    Task<PayrollRun?> GetRunByIdAsync(Guid id);
    Task<IEnumerable<Payslip>> GetPayslipsAsync(Guid runId);
    Task<ApiResponse<PayrollRun>> DraftPayrollAsync(DateTime periodStart, DateTime periodEnd);
    Task<ApiResponse<PayrollRun>> ApprovePayrollAsync(Guid runId);
    Task<ApiResponse> PayPayrollAsync(Guid runId);
}
