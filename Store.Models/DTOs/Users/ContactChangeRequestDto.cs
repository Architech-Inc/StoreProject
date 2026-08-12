using Store.Models.Enums;

namespace Store.Models.DTOs.Users;

public class ContactChangeRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? NewEmail { get; set; }
    public string? NewPhone { get; set; }
    public ContactChangeStatus Status { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? ApprovedByUsername { get; set; }
}
