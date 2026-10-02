namespace Store.Models.DTOs.Notifications;

public enum NotificationCategory
{
    General = 0,
    DiscountApproval = 1,
    LowStock = 2,
    PurchaseOrder = 3,
    ContactRequest = 4,
    Security = 5,
    CashVariance = 6,
    RestockRecommendation = 7
}

public class StoreNotificationDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationCategory Category { get; set; } = NotificationCategory.General;
    public string Severity { get; set; } = "Info"; // Info, Success, Warning, Danger
    public string? TargetUrl { get; set; }
    public string? ActionLabel { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class DiscountOverrideNotificationDto
{
    public Guid OverrideId { get; set; }
    public int DiscountOverrideRequestId { get; set; }
    public Guid? ItemId { get; set; }
    public string? PosSessionId { get; set; }
    public Guid CashierUserId { get; set; }
    public Guid? SupervisorUserId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled, Expired, Applied
    public decimal RequestedDiscount { get; set; }
    public string? Reason { get; set; }
    public string? SupervisorName { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public class LowStockAlertDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int? BranchId { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Broadcast whenever the velocity engine creates a new restock recommendation.
/// Carries enough denormalised info so the receiving client can update its UI
/// without a follow-up API round-trip.
/// </summary>
public class RestockRecommendationNotificationDto
{
    public Guid RecommendationId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemBarcode { get; set; }
    public int RecommendedQuantity { get; set; }
    public string Severity { get; set; } = "Medium";
    public int CurrentStock { get; set; }
    public double? DaysOfStock { get; set; }
    public decimal ProjectedValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public class CashVarianceAlertDto
{
    public Guid ShiftId { get; set; }
    public Guid CashierUserId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal ExpectedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal VarianceAmount { get; set; }
    public string Severity { get; set; } = "Warning"; // Info, Warning, Danger
    public string? Notes { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public class PurchaseOrderNotificationDto
{
    public int PurchaseOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // Submitted, Approved, PartiallyReceived, Received, Cancelled
    public Guid? RequestedByUserId { get; set; }
    public string? RequestedByName { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public class ContactRequestNotificationDto
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty; // Email, Phone, Email & Phone
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public Guid? ReviewedByUserId { get; set; }
    public string? Reason { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
