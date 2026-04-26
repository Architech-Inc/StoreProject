using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities.Contacts;

public class Email : BaseEntity
{
    public int EmailId { get; set; }
    public string Address { get; set; } = string.Empty;
    public EmailType Type { get; set; } = EmailType.Personal;
    public bool IsVerified { get; set; }

    public ICollection<UserEmail> UserEmails { get; set; } = new List<UserEmail>();
    public ICollection<EmployeeEmail> EmployeeEmails { get; set; } = new List<EmployeeEmail>();
    public ICollection<CustomerEmail> CustomerEmails { get; set; } = new List<CustomerEmail>();
    public ICollection<SupplierEmail> SupplierEmails { get; set; } = new List<SupplierEmail>();
    public ICollection<ManufacturerEmail> ManufacturerEmails { get; set; } = new List<ManufacturerEmail>();
}
