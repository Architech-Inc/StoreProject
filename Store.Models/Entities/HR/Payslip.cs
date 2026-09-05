using Store.Models.Entities.Base;

namespace Store.Models.Entities.HR;

public class Payslip : BaseEntity
{
    public Guid PayslipId { get; set; } = Guid.NewGuid();
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    
    public decimal BasicPay { get; set; }
    public decimal Allowances { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TaxDeducted { get; set; }
    public decimal NetPay { get; set; }
    
    // Navigation
    public PayrollRun? PayrollRun { get; set; }
    public Employee? Employee { get; set; }
}
