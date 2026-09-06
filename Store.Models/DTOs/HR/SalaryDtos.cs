using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.HR;

public class SalaryDto
{
    public int SalaryId { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal BasicAmount { get; set; }
    public decimal? AllowanceAmount { get; set; }
    public decimal TotalPackage => BasicAmount + (AllowanceAmount ?? 0);
    public string? Description { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateSalaryRequest
{
    [Required, StringLength(50)]
    public string Grade { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal BasicAmount { get; set; }

    [Range(0, 999999999)]
    public decimal? AllowanceAmount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateSalaryRequest : CreateSalaryRequest
{
}
