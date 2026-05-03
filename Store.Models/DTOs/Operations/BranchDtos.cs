using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Operations;

public class BranchDto
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
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
}

public class AssignUserBranchRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int RoleId { get; set; }
}

public class RemoveUserBranchRoleRequest
{
    [Required]
    public long UserBranchRoleId { get; set; }
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
