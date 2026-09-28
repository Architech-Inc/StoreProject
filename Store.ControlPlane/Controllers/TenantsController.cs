using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

using Microsoft.Extensions.Logging;
using Store.Models.Common;
namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ILogger<TenantsController> _logger;
    private readonly ITenantOrchestrator _orchestrator;
    private readonly IDbContextFactory<ControlPlaneDbContext> _dbContextFactory;
    private readonly ISecretEncryptionService _encryptionService;

    public TenantsController(
        ITenantOrchestrator orchestrator,
        IDbContextFactory<ControlPlaneDbContext> dbContextFactory,
        ISecretEncryptionService encryptionService,
        ILogger<TenantsController> logger)
    {
        _orchestrator = orchestrator;
        _dbContextFactory = dbContextFactory;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _orchestrator.GetAllTenantsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TenantDto>>.Ok(list));
    }

    [HttpGet("check-slug")]
    public async Task<IActionResult> CheckSlug(string slug, [FromServices] Store.ControlPlane.Repositories.ITenantRepository repo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length < 3)
        {
            return Ok(ApiResponse<SlugCheckDto>.Ok(new SlugCheckDto(slug, false, "Slug too short or empty.")));
        }
        var exists = await repo.SlugExistsAsync(slug.ToLowerInvariant(), ct);
        if (exists)
        {
            return Ok(ApiResponse<SlugCheckDto>.Ok(new SlugCheckDto(slug, false, "Slug is already taken.")));
        }
        return Ok(ApiResponse<SlugCheckDto>.Ok(new SlugCheckDto(slug, true, null)));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var summary = await _orchestrator.GetHealthSummaryAsync(ct);
        return Ok(ApiResponse<TenantHealthSummaryDto>.Ok(summary));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.GetTenantDetailsAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDetailDto>.Ok(tenant));
    }

    [HttpPost("provision")]
    public async Task<IActionResult> Provision([FromBody] ProvisionTenantRequest request, CancellationToken ct)
    {
        try
        {
            var tenant = await _orchestrator.ProvisionTenantAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = tenant.TenantId }, ApiResponse<TenantDto>.Ok(tenant, "Tenant stack provisioned successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(SafeErrorMessage.From(ex, _logger, "Tenants operation")));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Internal error during provisioning: {SafeErrorMessage.From(ex, _logger, "Tenants operation")}"));
        }
    }

    /// <summary>
    /// MT-01 — Async provisioning. Persists a job and returns immediately so
    /// the portal can poll <c>/provisioning/{jobId}</c> every 2-3 seconds.
    /// </summary>
    [HttpPost("provision-async")]
    public async Task<IActionResult> ProvisionAsync([FromBody] ProvisionTenantAsyncRequest request, CancellationToken ct)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

            var passwordCipher = _encryptionService.Encrypt(request.AdminPassword);

            var job = new TenantProvisioningJob
            {
                JobId = Guid.NewGuid(),
                AccountId = request.AccountId,
                StoreName = request.StoreName.Trim(),
                Slug = request.Slug.Trim().ToLowerInvariant(),
                AdminEmail = request.AdminEmail.Trim().ToLowerInvariant(),
                AdminUsername = request.AdminUsername.Trim(),
                AdminPasswordCipher = passwordCipher,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "XAF" : request.Currency.Trim(),
                PlanTier = request.PlanTier,
                CustomDomain = string.IsNullOrWhiteSpace(request.CustomDomain) ? null : request.CustomDomain.Trim(),
                Status = TenantProvisioningStatus.Pending,
                DateCreated = DateTime.UtcNow
            };

            db.TenantProvisioningJobs.Add(job);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("Provisioning job {JobId} queued for slug {Slug}", job.JobId, job.Slug);

            return Accepted(
                Url.Action(nameof(GetProvisioningJob), new { jobId = job.JobId }),
                ApiResponse<ProvisioningJobDto>.Ok(MapJob(job, tenantSlug: job.Slug), "Provisioning queued."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(SafeErrorMessage.From(ex, _logger, "Tenants async-provision operation")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue provisioning for slug {Slug}", request.Slug);
            return StatusCode(500, ApiResponse<object>.Fail($"Internal error while queueing provisioning: {SafeErrorMessage.From(ex, _logger, "Tenants async-provision operation")}"));
        }
    }

    /// <summary>MT-01 — Portal polls this every 2-3 seconds until Status is terminal.</summary>
    [HttpGet("provisioning/{jobId:guid}")]
    public async Task<IActionResult> GetProvisioningJob(Guid jobId, CancellationToken ct)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        var job = await db.TenantProvisioningJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.JobId == jobId, ct);

        if (job is null)
        {
            return NotFound(ApiResponse<object>.Fail("Provisioning job not found."));
        }

        string? tenantSlug = null;
        if (job.TenantId.HasValue)
        {
            tenantSlug = await db.Tenants
                .Where(t => t.TenantId == job.TenantId.Value)
                .Select(t => t.Slug)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(ApiResponse<ProvisioningJobDto>.Ok(MapJob(job, tenantSlug)));
    }

    /// <summary>MT-01 — Reset a Failed job back to Pending so the hosted service picks it up again.</summary>
    [HttpPost("provisioning/{jobId:guid}/retry")]
    public async Task<IActionResult> RetryProvisioningJob(Guid jobId, CancellationToken ct)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        var rows = await db.TenantProvisioningJobs
            .Where(j => j.JobId == jobId && j.Status == TenantProvisioningStatus.Failed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Status, TenantProvisioningStatus.Pending)
                .SetProperty(j => j.FailureReason, (string?)null)
                .SetProperty(j => j.CompletedAt, (DateTime?)null), ct);

        if (rows == 0)
        {
            return NotFound(ApiResponse<object>.Fail("Only Failed provisioning jobs can be retried."));
        }

        _logger.LogInformation("Provisioning job {JobId} reset to Pending for retry", jobId);
        return Accepted(
            Url.Action(nameof(GetProvisioningJob), new { jobId }),
            ApiResponse<object>.Ok(null!, "Provisioning re-queued."));
    }

    private static ProvisioningJobDto MapJob(TenantProvisioningJob job, string? tenantSlug = null) => new()
    {
        JobId = job.JobId,
        Status = job.Status.ToString(),
        StatusDetail = job.StatusDetail,
        FailureReason = job.FailureReason,
        TenantId = job.TenantId,
        TenantSlug = tenantSlug,
        DateCreated = job.DateCreated,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt
    };

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.SuspendTenantAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDto>.Ok(tenant, "Tenant stack suspended."));
    }

    [HttpPost("{id:guid}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.ResumeTenantAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDto>.Ok(tenant, "Tenant stack resumed."));
    }

    [HttpPost("{id:guid}/health")]
    public async Task<IActionResult> CheckHealth(Guid id, CancellationToken ct)
    {
        var isHealthy = await _orchestrator.CheckTenantHealthAsync(id, ct);
        return Ok(ApiResponse<bool>.Ok(isHealthy, isHealthy ? "Tenant healthy." : "Tenant check failed."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deprovision(Guid id, CancellationToken ct)
    {
        var success = await _orchestrator.DeprovisionTenantAsync(id, ct);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Tenant stack deprovisioned successfully."));
    }

    // ─── MT-07 — maintenance window operator endpoints (auth required) ─────

    /// <summary>
    /// Schedule a new maintenance window for a tenant. Operator-only.
    /// </summary>
    [HttpPost("{id:guid}/maintenance-windows")]
    public async Task<IActionResult> AddMaintenanceWindow(Guid id, [FromBody] CreateMaintenanceWindowRequest request, CancellationToken ct)
    {
        try
        {
            var actorEmail = User?.FindFirst("email")?.Value
                ?? User?.Identity?.Name
                ?? "operator";
            var dto = await _orchestrator.AddMaintenanceWindowAsync(id, request, actorEmail, ct);
            return Ok(ApiResponse<MaintenanceWindowDto>.Ok(dto, "Maintenance window scheduled."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(SafeErrorMessage.From(ex, _logger, "Schedule maintenance")));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.Fail(SafeErrorMessage.From(ex, _logger, "Schedule maintenance")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule maintenance window for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail(SafeErrorMessage.From(ex, _logger, "Schedule maintenance")));
        }
    }

    [HttpDelete("{id:guid}/maintenance-windows/{windowId:guid}")]
    public async Task<IActionResult> RemoveMaintenanceWindow(Guid id, Guid windowId, CancellationToken ct)
    {
        var ok = await _orchestrator.RemoveMaintenanceWindowAsync(id, windowId, ct);
        if (!ok)
        {
            return NotFound(ApiResponse<object>.Fail("Maintenance window not found."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Maintenance window removed."));
    }

    [HttpPost("{id:guid}/maintenance-windows/{windowId:guid}/resolve")]
    public async Task<IActionResult> ResolveMaintenanceWindow(Guid id, Guid windowId, CancellationToken ct)
    {
        var ok = await _orchestrator.ResolveMaintenanceWindowAsync(id, windowId, ct);
        if (!ok)
        {
            return NotFound(ApiResponse<object>.Fail("Maintenance window not found."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Maintenance window marked resolved."));
    }
}
