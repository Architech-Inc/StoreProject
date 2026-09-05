using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities.HR;

public class PayrollRun : BaseEntity
{
    public Guid PayrollRunId { get; set; } = Guid.NewGuid();
    
    public DateTime RunDate { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;
    
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalCommissions { get; set; }
    
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
