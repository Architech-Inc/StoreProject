using Store.Models.Entities.Base;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class User : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? EmployeeId { get; set; }
    public int RoleId { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.NotVerified;
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
    
    // Account Lockout
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    // 2FA Security
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }

    // Session Management
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    // Navigation properties
    public Employee? Employee { get; set; }
    public Role Role { get; set; } = null!;
    public UserPassword? Password { get; set; }
    public UserToken? UserToken { get; set; }

    public ICollection<UserEmail> Emails { get; set; } = new List<UserEmail>();
    public ICollection<UserPhone> Phones { get; set; } = new List<UserPhone>();
    public ICollection<UserPrivilege> Privileges { get; set; } = new List<UserPrivilege>();
    public ICollection<UserPrivilegeAction> PrivilegeActions { get; set; } = new List<UserPrivilegeAction>();
    public ICollection<EmployeePrivilegeAction> EmployeePrivilegeActions { get; set; } = new List<EmployeePrivilegeAction>();
    public ICollection<CustomerPrivilegeAction> CustomerPrivilegeActions { get; set; } = new List<CustomerPrivilegeAction>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<ItemsOrder> Orders { get; set; } = new List<ItemsOrder>();
    public ICollection<Discount> ManagedDiscounts { get; set; } = new List<Discount>();
    public ICollection<Otp> Otps { get; set; } = new List<Otp>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ChangeLog> ChangeLogs { get; set; } = new List<ChangeLog>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<CashierShift> OpenedShifts { get; set; } = new List<CashierShift>();
    public ICollection<CashierShift> ClosedShifts { get; set; } = new List<CashierShift>();
    public ICollection<FidoCredential> FidoCredentials { get; set; } = new List<FidoCredential>();
    
    public ICollection<ContactChangeRequest> ContactChangeRequests { get; set; } = new List<ContactChangeRequest>();
    public ICollection<ContactChangeRequest> ApprovedContactChanges { get; set; } = new List<ContactChangeRequest>();
}
