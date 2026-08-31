using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Workers;

public class LogRetentionWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<LogRetentionWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public LogRetentionWorker(
        IServiceProvider services,
        IConfiguration config,
        ILogger<LogRetentionWorker> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Log Retention & Maintenance Worker initialized.");

        // Initial delay to avoid slowing down container boot
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var retentionDays = _config.GetValue<int>("Audit:RetentionDays", 90);
                var threshold = DateTime.UtcNow.AddDays(-retentionDays);

                _logger.LogInformation("Executing log retention cleanup. Pruning logs older than {Threshold:u} ({Days} days retention)...", threshold, retentionDays);

                await using var scope = _services.CreateAsyncScope();
                var auditSvc = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                var commSvc = scope.ServiceProvider.GetRequiredService<ICommunicationLogService>();

                var prunedAudit = await auditSvc.PruneLogsOlderThanAsync(threshold, stoppingToken);
                var prunedComm = await commSvc.PruneLogsOlderThanAsync(threshold, stoppingToken);

                _logger.LogInformation("Log retention maintenance completed: Pruned {AuditCount} audit logs and {CommCount} communication logs.", prunedAudit, prunedComm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during log retention pruning cycle.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Log Retention & Maintenance Worker stopped.");
    }
}
