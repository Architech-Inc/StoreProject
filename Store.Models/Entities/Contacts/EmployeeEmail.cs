using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class EmployeeEmail : BaseEntity
{
    public int EmployeeEmailId { get; set; }
    public Guid EmployeeId { get; set; }
    public int EmailId { get; set; }
    public bool IsPrimary { get; set; }

    public Employee Employee { get; set; } = null!;
    public Email Email { get; set; } = null!;
}
