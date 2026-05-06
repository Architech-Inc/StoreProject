namespace Store.Models.Enums;

public enum UserStatus
{
    NotVerified,
    Verified,
    Active,
    Banned,
    Suspended,
    Deactivated,
    Deleted
}

public enum EmployeeStatus
{
    Pending,
    Active,
    Suspended,
    Sanctioned,
    Fired,
    Volunteered
}

public enum Gender
{
    NotSpecified,
    Male,
    Female,
    Other
}

public enum EmailType
{
    Personal,
    Business,
    Work,
    School
}

public enum PhoneType
{
    Mobile,
    Home,
    Work,
    Fax
}

public enum OtpPurpose
{
    PasswordReset,
    EmailVerification,
    PhoneVerification,
    TwoFactorLogin
}

public enum PaymentType
{
    Cash,
    Card,
    BankTransfer,
    MobileMoney,
    Cheque,
    Credit
}

public enum OrderStatus
{
    Draft,
    Pending,
    Approved,
    PartiallyReceived,
    Received,
    Cancelled
}

public enum ItemType
{
    Product,
    Service,
    Digital
}

public enum StockMovementType
{
    Receive,
    Sale,
    Return,
    Adjustment,
    Void
}

public enum CustomerSegment
{
    Standard,
    Wholesale,
    Vip
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}

public enum StockTransferStatus
{
    Requested = 0,
    Approved = 1,
    Dispatched = 2,
    Received = 3,
    Cancelled = 4
}

/// <summary>Reason type for a wastage/shrinkage stock write-off (INV-3).</summary>
public enum WastageType
{
    Damage = 0,
    Expiry = 1,
    Theft = 2,
    Spoilage = 3,
    AdminError = 4,
    Other = 5
}

/// <summary>Status of a manager discount override request (EX-FR-3.3).</summary>
public enum DiscountOverrideStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

/// <summary>Lifecycle status of a purchase order (EX-FR-1.2).</summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    PartiallyReceived = 3,
    Received = 4,
    Cancelled = 5
}

/// <summary>Status of a cash-variance record requiring manager review (EX-FR-5.2).</summary>
public enum CashVarianceStatus
{
    Pending = 0,
    Reviewed = 1,
    Escalated = 2
}

public enum TaxApplicationType
{
    Exclusive,
    Inclusive
}

public enum ShiftStatus
{
    Open,
    Closed
}

public enum MobileMoneyProvider
{
    MtnMomo,
    OrangeMoney
}

public enum MobileMoneyStatus
{
    Pending,
    Completed,
    Failed,
    TimedOut
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}

public enum ChangeLogAction
{
    Created,
    Updated,
    Deleted,
    Restored
}

public enum PrivilegeType
{
    Read,
    Write,
    Delete,
    Manage
}

public enum InfoType
{
    General,
    FoundObject,
    ResetPassword,
    Suspended,
    Banned,
    VerifiedEmail,
    VerifiedPhone
}
