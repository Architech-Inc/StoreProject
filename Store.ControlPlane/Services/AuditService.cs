using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;

namespace Store.ControlPlane.Services;

public class AuditService : IAuditService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ITenantRepository tenantRepo, ILogger<AuditService> logger)
    {
        _tenantRepo = tenantRepo;
        _logger = logger;
    }

    public async Task RecordAuditAsync(Guid tenantId, string actionType, string actorEmail, string details, string? ipAddress = null, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null)
        {
            _logger.LogWarning("Cannot record audit for non-existent tenant {TenantId}", tenantId);
            return;
        }

        tenant.AuditTrail ??= new List<TenantAuditRecord>();

        var record = new TenantAuditRecord
        {
            AuditId = Guid.NewGuid(),
            TenantId = tenantId,
            Timestamp = DateTime.UtcNow,
            ActorEmail = string.IsNullOrWhiteSpace(actorEmail) ? tenant.AdminEmail : actorEmail,
            ActionType = actionType,
            Details = details,
            IpAddress = ipAddress
        };

        tenant.AuditTrail.Insert(0, record);

        // Keep last 200 audit entries
        if (tenant.AuditTrail.Count > 200)
        {
            tenant.AuditTrail = tenant.AuditTrail.Take(200).ToList();
        }

        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Audit [{ActionType}] recorded for tenant {Slug} by {Actor}: {Details}", actionType, tenant.Slug, actorEmail, details);
    }

    public async Task<IReadOnlyList<TenantAuditDto>> GetAuditTrailAsync(Guid tenantId, int limit = 50, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant?.AuditTrail == null) return Array.Empty<TenantAuditDto>();

        return tenant.AuditTrail
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new TenantAuditDto(
                a.AuditId,
                a.TenantId,
                a.Timestamp,
                a.ActorEmail,
                a.ActionType,
                a.Details,
                a.IpAddress
            ))
            .ToList();
    }
}
