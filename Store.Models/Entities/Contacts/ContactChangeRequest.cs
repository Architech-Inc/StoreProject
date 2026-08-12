using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities.Contacts;

public class ContactChangeRequest : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    
    public string? NewEmail { get; set; }
    public string? NewPhone { get; set; }
    
    public string? VerificationToken { get; set; }
    
    public ContactChangeStatus Status { get; set; } = ContactChangeStatus.PendingVerification;
    
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedById { get; set; }
    
    public User User { get; set; } = null!;
    public User? ApprovedBy { get; set; }
}
