using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Department : BaseEntity
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
