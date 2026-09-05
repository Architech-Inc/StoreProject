using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Operations;

public class BranchDto
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public decimal PriceMultiplier { get; set; } = 1.0m;
    public decimal? TaxRateOverride { get; set; }
}

public class UpsertBranchRequest
{
    public int? BranchId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
    public decimal? PriceMultiplier { get; set; } = 1.0m;
    public decimal? TaxRateOverride { get; set; }
}

public class UserBranchRoleDto
{
    public long UserBranchRoleId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? GrantReason { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsExpired => ValidTo.HasValue && ValidTo.Value < DateTime.UtcNow;
}

public class AssignUserBranchRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int RoleId { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    [StringLength(255)]
    public string? GrantReason { get; set; }
}

public class RemoveUserBranchRoleRequest
{
    [Required]
    public long UserBranchRoleId { get; set; }
}

public class TransferEmployeeRequest
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public int ToBranchId { get; set; }

    public int? DestinationRoleId { get; set; }

    public int TransferType { get; set; } = 0; // 0 = Permanent, 1 = Temporary, 2 = Training

    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedEndDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class PersonnelTransferDto
{
    public long PersonnelTransferId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int FromBranchId { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public int ToBranchId { get; set; }
    public string ToBranchName { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public string? Reason { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime DateCreated { get; set; }
}

public class BranchItemStockDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int InStock { get; set; }
    public int? ReorderLevel { get; set; }
    public int? ReorderQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal BaseUnitPrice { get; set; }
    public decimal? CustomUnitPrice { get; set; }
    public decimal EffectiveUnitPrice { get; set; }
    public decimal? CustomCostPrice { get; set; }
}

public class UpdateBranchStockRequest
{
    [Required]
    public int BranchId { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    public int? InStockDelta { get; set; }
    public int? AbsoluteInStock { get; set; }
    public int? ReorderLevel { get; set; }
    public int? ReorderQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? CustomUnitPrice { get; set; }
    public decimal? CustomCostPrice { get; set; }
}

public class BranchPerformanceDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public decimal OutstandingBalance { get; set; }
    public IReadOnlyList<PaymentTypeSummary> RevenueByPaymentType { get; set; } = [];
    public IReadOnlyList<DailyRevenueSummary> DailyRevenue { get; set; } = [];
}

public class PaymentTypeSummary
{
    public string PaymentType { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal Total { get; set; }
}

public class DailyRevenueSummary
{
    public DateOnly Date { get; set; }
    public int InvoiceCount { get; set; }
    public decimal Total { get; set; }
}
