using Store.Models.Entities.HR;

namespace Store.DbServices.Services.Interfaces;

public interface IPayrollService
{
    Task<PayrollRun> DraftPayrollRunAsync(DateTime periodStart, DateTime periodEnd);
    Task<PayrollRun> ApprovePayrollAsync(Guid payrollRunId, Guid approvedByUserId);
    Task<bool> PayPayrollRunAsync(Guid payrollRunId);
    Task<IEnumerable<PayrollRun>> GetAllPayrollRunsAsync();
    Task<PayrollRun?> GetPayrollRunByIdAsync(Guid payrollRunId);
    Task<IEnumerable<Payslip>> GetPayslipsForRunAsync(Guid payrollRunId);
}
