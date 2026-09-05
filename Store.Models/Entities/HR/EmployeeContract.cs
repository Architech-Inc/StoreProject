using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities.HR;

public class EmployeeContract : BaseEntity
{
    public Guid EmployeeContractId { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public int? SalaryId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public PayrollType PayrollType { get; set; } = PayrollType.Taxed;
    public bool CalculateTaxOnGross { get; set; } = true;

    // Navigation
    public Employee? Employee { get; set; }
    public Salary? Salary { get; set; }
}
