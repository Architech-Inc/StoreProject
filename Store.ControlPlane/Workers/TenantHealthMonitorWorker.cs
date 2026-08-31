using Store.ControlPlane.Models;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;

namespace Store.ControlPlane.Workers;

public class TenantHealthMonitorWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TenantHealthMonitorWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(60);

    public TenantHealthMonitorWorker(IServiceProvider services, ILogger<TenantHealthMonitorWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tenant Health Monitor Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                var orchestrator = scope.ServiceProvider.GetRequiredService<ITenantOrchestrator>();

                var activeTenants = (await repo.GetAllAsync(stoppingToken))
                    .Where(t => t.Status == TenantStatus.Active)
                    .ToList();

                foreach (var tenant in activeTenants)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    await orchestrator.CheckTenantHealthAsync(tenant.TenantId, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Tenant Health Monitor execution loop.");
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

        _logger.LogInformation("Tenant Health Monitor Worker stopped.");
    }
}
