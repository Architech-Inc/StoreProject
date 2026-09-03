using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/sdlc")]
public class SdlcController : ControllerBase
{
    private readonly ITenantOrchestrator _orchestrator;
    private readonly ControlPlaneDbContext _dbContext;

    public SdlcController(ITenantOrchestrator orchestrator, ControlPlaneDbContext dbContext)
    {
        _orchestrator = orchestrator;
        _dbContext = dbContext;
    }

    [HttpGet("releases")]
    public async Task<IActionResult> GetReleases(CancellationToken ct)
    {
        var releases = await _dbContext.SystemReleases
            .OrderByDescending(r => r.ReleaseDate)
            .Select(r => new SystemReleaseDto
            {
                ReleaseId = r.ReleaseId,
                VersionName = r.VersionName,
                ReleaseDate = r.ReleaseDate,
                IsPublic = r.IsPublic,
                ReleaseNotes = r.ReleaseNotes
            })
            .ToListAsync(ct);

        return Ok(releases);
    }

    [HttpPost("tenants/{slug}/upgrade/{releaseId:guid}")]
    public async Task<IActionResult> UpgradeTenant(string slug, Guid releaseId, CancellationToken ct)
    {
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tenant == null) return NotFound();

        var release = await _dbContext.SystemReleases.FindAsync(new object[] { releaseId }, ct);
        if (release == null) return NotFound("Release not found");

        // 1. Create pre-upgrade snapshot
        await _orchestrator.CreateSnapshotAsync(tenant.TenantId, SnapshotType.PreUpgrade, ct);

        // 2. Set new release ID
        tenant.CurrentReleaseId = releaseId;
        await _dbContext.SaveChangesAsync(ct);

        // 3. Restart containers (this triggers RenderComposeTemplate with the new release)
        await _orchestrator.RestartAllContainersAsync(tenant.TenantId, ct);

        return Ok();
    }

    [HttpPost("tenants/{slug}/rollback/{snapshotId:guid}")]
    public async Task<IActionResult> RollbackTenant(string slug, Guid snapshotId, CancellationToken ct)
    {
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tenant == null) return NotFound();

        await _orchestrator.RestoreSnapshotAsync(tenant.TenantId, snapshotId, ct);

        return Ok();
    }

    [HttpPost("tenants/{slug}/sandbox/{releaseId:guid}")]
    public async Task<IActionResult> CreateSandbox(string slug, Guid releaseId, [FromQuery] bool maskData = true, CancellationToken ct = default)
    {
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tenant == null) return NotFound();

        var sandboxDto = await _orchestrator.ProvisionSandboxAsync(tenant.TenantId, releaseId, maskData, ct);

        return Ok(sandboxDto);
    }

    [HttpGet("tenants/{slug}/status")]
    public async Task<IActionResult> GetTenantSdlcStatus(string slug, CancellationToken ct)
    {
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tenant == null) return NotFound("Tenant not found");

        var allReleases = await _dbContext.SystemReleases
            .OrderByDescending(r => r.ReleaseDate)
            .Select(r => new SystemReleaseDto
            {
                ReleaseId = r.ReleaseId,
                VersionName = r.VersionName,
                ReleaseDate = r.ReleaseDate,
                IsPublic = r.IsPublic,
                ReleaseNotes = r.ReleaseNotes
            })
            .ToListAsync(ct);

        SystemReleaseDto? currentRelease = null;
        if (tenant.CurrentReleaseId.HasValue)
        {
            currentRelease = allReleases.FirstOrDefault(r => r.ReleaseId == tenant.CurrentReleaseId.Value);
        }

        var snapshots = await _dbContext.TenantSnapshots
            .Where(s => s.TenantId == tenant.TenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new TenantSnapshotDto
            {
                SnapshotId = s.SnapshotId,
                TenantId = s.TenantId,
                ReleaseId = s.ReleaseId,
                Type = s.Type.ToString(),
                CreatedAt = s.CreatedAt,
                SizeBytes = s.SizeBytes
            })
            .ToListAsync(ct);

        var sandboxes = await _dbContext.Tenants
            .Where(t => t.ParentTenantId == tenant.TenantId)
            .Select(s => new SandboxSummaryDto
            {
                TenantId = s.TenantId,
                Name = s.Name,
                Slug = s.Slug,
                UiUrl = s.UiUrl,
                ApiUrl = s.ApiUrl,
                ReleaseId = s.CurrentReleaseId,
                ReleaseVersion = s.CurrentReleaseId.HasValue ? _dbContext.SystemReleases.Where(r => r.ReleaseId == s.CurrentReleaseId.Value).Select(r => r.VersionName).FirstOrDefault() : "Custom",
                DateCreated = s.DateCreated,
                IsHealthy = s.IsHealthy
            })
            .ToListAsync(ct);

        string? parentSlug = null;
        if (tenant.ParentTenantId.HasValue)
        {
            parentSlug = await _dbContext.Tenants
                .Where(t => t.TenantId == tenant.ParentTenantId.Value)
                .Select(t => t.Slug)
                .FirstOrDefaultAsync(ct);
        }

        var status = new TenantSdlcStatusDto
        {
            TenantId = tenant.TenantId,
            Slug = tenant.Slug,
            CurrentReleaseId = tenant.CurrentReleaseId,
            CurrentRelease = currentRelease,
            EnvironmentType = tenant.EnvironmentType.ToString(),
            ParentTenantId = tenant.ParentTenantId,
            ParentSlug = parentSlug,
            LastAccessedAt = tenant.LastAccessedAt,
            AvailableReleases = allReleases,
            Snapshots = snapshots,
            Sandboxes = sandboxes
        };

        return Ok(status);
    }

    [HttpGet("tenants/{slug}/snapshots")]
    public async Task<IActionResult> GetTenantSnapshots(string slug, CancellationToken ct)
    {
        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tenant == null) return NotFound("Tenant not found");

        var snapshots = await _dbContext.TenantSnapshots
            .Where(s => s.TenantId == tenant.TenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new TenantSnapshotDto
            {
                SnapshotId = s.SnapshotId,
                TenantId = s.TenantId,
                ReleaseId = s.ReleaseId,
                Type = s.Type.ToString(),
                CreatedAt = s.CreatedAt,
                SizeBytes = s.SizeBytes
            })
            .ToListAsync(ct);

        return Ok(snapshots);
    }

    [HttpDelete("tenants/{slug}/sandbox/{sandboxSlug}")]
    public async Task<IActionResult> DeleteSandbox(string slug, string sandboxSlug, CancellationToken ct)
    {
        var parent = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (parent == null) return NotFound("Parent tenant not found");

        var sandbox = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == sandboxSlug && t.ParentTenantId == parent.TenantId, ct);
        if (sandbox == null) return NotFound("Sandbox not found or does not belong to this tenant");

        await _orchestrator.DeprovisionTenantAsync(sandbox.TenantId, ct);
        return Ok(new { Message = $"Sandbox '{sandboxSlug}' decommissioned successfully." });
    }
}
