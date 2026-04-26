using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Employees;

public class EmployeeDto
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {(MiddleName is not null ? MiddleName + " " : "")}{LastName}".Trim();
    public string? NidNumber { get; set; }
    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? SalaryId { get; set; }
    public string? SalaryGrade { get; set; }
    public EmployeeStatus Status { get; set; }
    public DateTime DateEmployed { get; set; }
    public string? ImagePath { get; set; }
    public DateTime DateCreated { get; set; }
}

public class CreateEmployeeRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MiddleName { get; set; }

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; } = Gender.NotSpecified;
    public DateTime? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? NidNumber { get; set; }

    [Required]
    public DateTime DateEmployed { get; set; }

    public int? DepartmentId { get; set; }
    public int? SalaryId { get; set; }
}

public class UpdateEmployeeRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? MiddleName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? DepartmentId { get; set; }
    public int? SalaryId { get; set; }
    public EmployeeStatus? Status { get; set; }
}
