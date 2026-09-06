using Store.Models.Entities.Base;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Employee : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? SalaryId { get; set; }
    public int? HomeBranchId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? NidNumber { get; set; }
    public Gender Gender { get; set; } = Gender.NotSpecified;
    public DateTime? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public DateTime DateEmployed { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Pending;
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }

    // Navigation
    public Department? Department { get; set; }
    public Salary? Salary { get; set; }
    public Branch? HomeBranch { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<EmployeeEmail> Emails { get; set; } = new List<EmployeeEmail>();
    public ICollection<EmployeePhone> Phones { get; set; } = new List<EmployeePhone>();
    public ICollection<PersonnelTransferHistory> TransferHistories { get; set; } = new List<PersonnelTransferHistory>();
    public ICollection<Store.Models.Entities.HR.EmployeeContract> Contracts { get; set; } = new List<Store.Models.Entities.HR.EmployeeContract>();
}
