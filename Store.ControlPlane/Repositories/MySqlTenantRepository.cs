using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;

namespace Store.ControlPlane.Repositories;

public class MySqlTenantRepository : ITenantRepository
{
    private readonly IDbContextFactory<ControlPlaneDbContext> _contextFactory;
    private readonly ILogger<MySqlTenantRepository> _logger;

    public MySqlTenantRepository(
        IDbContextFactory<ControlPlaneDbContext> contextFactory,
        ILogger<MySqlTenantRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        return await db.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.DateCreated)
            .ToListAsync(ct);
    }

    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        return await db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        return await db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug.ToLower() == slug.ToLower(), ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        return await db.Tenants
            .AnyAsync(t => t.Slug.ToLower() == slug.ToLower(), ct);
    }

    public async Task SaveAsync(Tenant tenant, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var existing = await db.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId, ct);

        if (existing == null)
        {
            db.Tenants.Add(tenant);
            _logger.LogInformation("Creating new tenant in database: {Slug} ({Id})", tenant.Slug, tenant.TenantId);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(tenant);
            existing.Secrets = tenant.Secrets;
            existing.DomainConfig = tenant.DomainConfig;
            existing.Branches = tenant.Branches;
            existing.BackupProviders = tenant.BackupProviders;
            existing.BackupSchedule = tenant.BackupSchedule;
            existing.BackupHistory = tenant.BackupHistory;
            existing.AuditTrail = tenant.AuditTrail;
            existing.ProvisioningLogs = tenant.ProvisioningLogs;
            _logger.LogInformation("Updating tenant in database: {Slug} ({Id})", tenant.Slug, tenant.TenantId);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var existing = await db.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
        if (existing != null)
        {
            db.Tenants.Remove(existing);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Deleted tenant from database: {Slug} ({Id})", existing.Slug, existing.TenantId);
        }
    }
}
