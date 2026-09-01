namespace Store.ControlPlane.Models.DTOs;

public record TenantAuditDto(
    Guid AuditId,
    Guid TenantId,
    DateTime Timestamp,
    string ActorEmail,
    string ActionType,
    string Details,
    string? IpAddress
);

public record AddAuditRecordRequest(
    string ActionType,
    string ActorEmail,
    string Details,
    string? IpAddress = null
);
