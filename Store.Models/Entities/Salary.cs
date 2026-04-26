using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Salary : BaseEntity
{
    public int SalaryId { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal BasicAmount { get; set; }
    public decimal? AllowanceAmount { get; set; }
    public string? Description { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
