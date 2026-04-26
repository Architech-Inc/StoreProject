using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities.Contacts;

public class Phone : BaseEntity
{
    public int PhoneId { get; set; }
    public int CountryId { get; set; }
    public string Number { get; set; } = string.Empty;
    public PhoneType Type { get; set; } = PhoneType.Mobile;
    public bool IsVerified { get; set; }

    public ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>();
    public ICollection<EmployeePhone> EmployeePhones { get; set; } = new List<EmployeePhone>();
    public ICollection<CustomerPhone> CustomerPhones { get; set; } = new List<CustomerPhone>();
    public ICollection<SupplierPhone> SupplierPhones { get; set; } = new List<SupplierPhone>();
    public ICollection<ManufacturerPhone> ManufacturerPhones { get; set; } = new List<ManufacturerPhone>();
}
