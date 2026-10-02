using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;

namespace Store.ControlPlane.Services;

/// <summary>
/// MT-01 — Background processor for <see cref="TenantProvisioningJob"/> rows.
///
/// <para>
/// Polls every 3 seconds (when the queue is non-empty), claims the oldest
/// Pending job, runs the existing <see cref="ITenantOrchestrator.ProvisionTenantAsync"/>
/// pipeline, then links the resulting tenant to the portal account.
/// </para>
/// <para>
/// Security:
///   - The job stores the admin password as <c>AdminPasswordCipher</c>, AES-GCM
///     encrypted under <c>ControlPlane:MasterEncryptionKey</c>. The job row
///     never holds the plaintext at rest.
///   - The hosted service decrypts in-memory only when running the
///     provisioning pipeline, then forgets it.
/// </para>
/// <para>
/// Failure modes:
///   - Provisioning throws → job marked Failed, FailureReason set, the user
///     can retry from the status page without re-entering form data (the job
///     is reusable: just reset Status=Pending).
///   - DB unreachable → service logs and continues on the next tick.
/// </para>
/// </summary>
public class TenantProvisioningHostedService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);
    private const int MaxAttempts = 3;

    private readonly IServiceProvider _services;
    private readonly ILogger<TenantProvisioningHostedService> _logger;

    public TenantProvisioningHostedService(
        IServiceProvider services,
        ILogger<TenantProvisioningHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TenantProvisioningHostedService started; polling every {Seconds}s", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessQueueAsync(stoppingToken);
                if (processed == 0)
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                // else: drain immediately
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TenantProvisioningHostedService tick failed");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }

    private async Task<int> ProcessQueueAsync(CancellationToken ct)
    {
        // Each iteration is its own DI scope so the DbContext + orchestrator
        // get fresh instances — the hosted service itself is a singleton.
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ControlPlaneDbContext>();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ITenantOrchestrator>();
        var encryption = scope.ServiceProvider.GetService<ISecretEncryptionService>();
        var authService = scope.ServiceProvider.GetRequiredService<IPortalAuthService>();

        var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

        // Atomically claim the oldest Pending job — UPDATE ... WHERE Status=Pending
        // returns 0 rows for races so a second hosted-service instance won't double-process.
        var job = await db.TenantProvisioningJobs
            .Where(j => j.Status == TenantProvisioningStatus.Pending)
            .OrderBy(j => j.DateCreated)
            .FirstOrDefaultAsync(ct);

        if (job is null) return 0;

        var claimed = await db.TenantProvisioningJobs
            .Where(j => j.JobId == job.JobId && j.Status == TenantProvisioningStatus.Pending)
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Status, TenantProvisioningStatus.InProgress)
                .SetProperty(j => j.StartedAt, DateTime.UtcNow), ct);

        if (claimed == 0) return 1; // raced; try next tick

        _logger.LogInformation("Provisioning job {JobId} for slug {Slug} (account {AccountId}) picked up",
            job.JobId, job.Slug, job.AccountId);

        var attempt = 0;
        Exception? lastException = null;
        while (attempt < MaxAttempts)
        {
            attempt++;
            try
            {
                var plaintextPassword = encryption is null
                    ? throw new InvalidOperationException("ISecretEncryptionService is not registered; cannot decrypt the admin password from the job row.")
                    : encryption.Decrypt(job.AdminPasswordCipher);

                // Idempotency: Check if the tenant was already provisioned in an earlier attempt or step
                var existingTenant = await tenantRepo.GetBySlugAsync(job.Slug, ct);
                TenantDto tenant;
                if (existingTenant != null && existingTenant.AdminEmail.Equals(job.AdminEmail, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Tenant {Slug} already provisioned for account {AccountId}, reusing existing tenant record.", job.Slug, job.AccountId);
                    tenant = new TenantDto
                    {
                        TenantId = existingTenant.TenantId,
                        Name = existingTenant.Name,
                        Slug = existingTenant.Slug,
                        AdminEmail = existingTenant.AdminEmail,
                        AdminUsername = existingTenant.AdminUsername,
                        Currency = existingTenant.Currency,
                        Status = existingTenant.Status,
                        PlanTier = existingTenant.PlanTier,
                        CustomDomain = existingTenant.CustomDomain,
                        UiUrl = existingTenant.UiUrl,
                        ApiUrl = existingTenant.ApiUrl,
                        DateCreated = existingTenant.DateCreated,
                        LastHealthCheck = existingTenant.LastHealthCheck,
                        IsHealthy = existingTenant.IsHealthy,
                        LastHealthMessage = existingTenant.LastHealthMessage
                    };
                }
                else
                {
                    tenant = await orchestrator.ProvisionTenantAsync(new ProvisionTenantRequest
                    {
                        StoreName = job.StoreName,
                        Slug = job.Slug,
                        AdminEmail = job.AdminEmail,
                        AdminUsername = job.AdminUsername,
                        AdminPassword = plaintextPassword,
                        Currency = job.Currency,
                        PlanTier = job.PlanTier,
                        CustomDomain = job.CustomDomain
                    }, ct);
                }

                // Link the account to the freshly-provisioned tenant.
                await authService.LinkAccountToTenantAsync(job.AccountId, tenant.TenantId, ct);

                // Mark Completed using tracked entity / SaveChangesAsync
                var completedJob = await db.TenantProvisioningJobs.FirstOrDefaultAsync(j => j.JobId == job.JobId, ct);
                if (completedJob != null)
                {
                    completedJob.Status = TenantProvisioningStatus.Completed;
                    completedJob.TenantId = tenant.TenantId;
                    completedJob.StatusDetail = $"Tenant '{tenant.Slug}' provisioned at {DateTime.UtcNow:O}.";
                    completedJob.CompletedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(ct);
                }

                _logger.LogInformation("Provisioning job {JobId} completed -> tenant {TenantId}", job.JobId, tenant.TenantId);
                return 1;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Provisioning job {JobId} attempt {Attempt} failed", job.JobId, attempt);
                if (attempt < MaxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                }
            }
        }

        // All attempts exhausted — mark Failed.
        var failureReason = lastException?.Message ?? "Unknown error.";
        var failedJob = await db.TenantProvisioningJobs.FirstOrDefaultAsync(j => j.JobId == job.JobId, ct);
        if (failedJob != null)
        {
            failedJob.Status = TenantProvisioningStatus.Failed;
            failedJob.FailureReason = failureReason;
            failedJob.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        _logger.LogError(lastException, "Provisioning job {JobId} failed after {Attempts} attempts", job.JobId, MaxAttempts);
        return 1;
    }
}
