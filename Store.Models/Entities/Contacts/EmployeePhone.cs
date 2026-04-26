using Store.Models.Entities.Base;

namespace Store.Models.Entities.Contacts;

public class EmployeePhone : BaseEntity
{
    public int EmployeePhoneId { get; set; }
    public Guid EmployeeId { get; set; }
    public int PhoneId { get; set; }
    public bool IsPrimary { get; set; }

    public Employee Employee { get; set; } = null!;
    public Phone Phone { get; set; } = null!;
}
