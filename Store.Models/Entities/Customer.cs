using Store.Models.Entities.Base;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Customer : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.NotSpecified;
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }

    // Navigation
    public ICollection<CustomerEmail> Emails { get; set; } = new List<CustomerEmail>();
    public ICollection<CustomerPhone> Phones { get; set; } = new List<CustomerPhone>();
    public ICollection<CustomerLocation> Locations { get; set; } = new List<CustomerLocation>();
    public ICollection<CustomerPrivilege> Privileges { get; set; } = new List<CustomerPrivilege>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
