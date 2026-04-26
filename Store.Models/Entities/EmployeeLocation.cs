using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class EmployeeLocation : BaseEntity
{
    public int EmployeeLocationId { get; set; }
    public Guid EmployeeId { get; set; }
    public int LocationId { get; set; }
    public bool IsPrimary { get; set; }

    public Employee Employee { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
