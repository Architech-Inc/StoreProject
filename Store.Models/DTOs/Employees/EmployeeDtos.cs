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
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
    public DateTime DateCreated { get; set; }
    public string ShortEmployeeCode => "EMP-" + EmployeeId.ToString("N")[..8].ToUpperInvariant();
    public string TenureDisplay
    {
        get
        {
            if (DateEmployed == default) return "—";
            var now = DateTime.Today;
            if (DateEmployed > now) return "Upcoming";
            var totalDays = (int)(now - DateEmployed.Date).TotalDays;
            var years = now.Year - DateEmployed.Year;
            if (now.Month < DateEmployed.Month || (now.Month == DateEmployed.Month && now.Day < DateEmployed.Day))
                years--;
            var months = now.Month - DateEmployed.Month;
            if (now.Day < DateEmployed.Day) months--;
            if (months < 0) months += 12;

            if (years >= 1)
                return months > 0 ? $"{years}y {months}m" : $"{years}y";
            if (months >= 1)
                return $"{months}m";
            return $"{totalDays}d";
        }
    }
}

public class EmployeeMetricsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PendingEmployees { get; set; }
    public int TerminatedEmployees { get; set; }
    public int DepartmentCount { get; set; }
}

public class EmployeeFilterRequest : Common.PagedRequest
{
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
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
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
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
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
}
