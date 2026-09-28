using Store.ControlPlane.Models;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;

namespace Store.ControlPlane.Workers;

/// <summary>
/// MT-06 — background worker that runs per-tenant backups on each tenant's
/// configured schedule.
///
/// Polls every 30s (cheap), evaluates every active tenant's
/// <see cref="BackupScheduleConfig"/>, and fires
/// <see cref="IBackupService.TriggerBackupNowAsync"/> when the next-run time
/// has passed. Updates <see cref="BackupScheduleConfig.LastRunAt"/> +
/// <see cref="BackupScheduleConfig.NextRunAt"/> + <c>LastRunStatus</c> after
/// each run.
///
/// Retention cleanup runs inline inside <c>TriggerBackupNowAsync</c> so we
/// don't need a separate sweep.
/// </summary>
public class TenantBackupHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TenantBackupHostedService> _logger;
    private readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(30);

    public TenantBackupHostedService(IServiceProvider services, ILogger<TenantBackupHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tenant Backup Scheduler started (tick = {Interval}).", _tickInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunDueSchedulesAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                // Never let the worker die from one tenant's failure.
                _logger.LogError(ex, "Tenant Backup Scheduler tick failed.");
            }

            try
            {
                await Task.Delay(_tickInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Tenant Backup Scheduler stopped.");
    }

    private async Task RunDueSchedulesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

        var now = DateTime.UtcNow;
        var tenants = (await tenantRepo.GetAllAsync(ct))
            .Where(t => t.Status == TenantStatus.Active)
            .ToList();

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested) break;

            var schedule = tenant.BackupSchedule;
            if (schedule is null) continue;

            if (!BackupScheduleEvaluator.ShouldRunNow(schedule, now))
            {
                continue;
            }

            try
            {
                await backupService.TriggerBackupNowAsync(tenant.TenantId, ct);

                // Update schedule state after successful trigger.
                schedule.LastRunAt = now;
                schedule.LastRunStatus = "Success";
                schedule.NextRunAt = BackupScheduleEvaluator.ComputeNextRunAt(schedule, now, now);

                tenant.ProvisioningLogs.Add(new TenantProvisioningLog
                {
                    LogId = Guid.NewGuid(),
                    TenantId = tenant.TenantId,
                    StepName = "ScheduledBackup",
                    Message = $"Schedule fired ({schedule.Frequency}); next run at {schedule.NextRunAt:O}"
                });

                await tenantRepo.SaveAsync(tenant, ct);
                _logger.LogInformation("Tenant {Slug}: scheduled backup fired; next at {NextRun}", tenant.Slug, schedule.NextRunAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tenant {Slug}: scheduled backup failed.", tenant.Slug);

                try
                {
                    schedule.LastRunAt = now;
                    schedule.LastRunStatus = "Failed: " + ex.GetType().Name;
                    schedule.NextRunAt = BackupScheduleEvaluator.ComputeNextRunAt(schedule, now, now);
                    tenant.ProvisioningLogs.Add(new TenantProvisioningLog
                    {
                        LogId = Guid.NewGuid(),
                        TenantId = tenant.TenantId,
                        StepName = "ScheduledBackupFailed",
                        Message = ex.Message
                    });
                    await tenantRepo.SaveAsync(tenant, ct);
                }
                catch (Exception saveEx)
                {
                    _logger.LogWarning(saveEx, "Tenant {Slug}: failed to persist failure state.", tenant.Slug);
                }
            }
        }
    }
}