using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Audit record of an employee's transfer or temporary rotation between branches.
/// </summary>
public class PersonnelTransferHistory : BaseEntity
{
    [Key]
    public long PersonnelTransferId { get; set; }

    public Guid EmployeeId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }

    public PersonnelTransferType TransferType { get; set; } = PersonnelTransferType.Permanent;
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedEndDate { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public Branch FromBranch { get; set; } = null!;
    public Branch ToBranch { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
