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
