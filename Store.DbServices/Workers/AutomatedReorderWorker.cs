using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Store.Models.DTOs.Notifications;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Workers;

public class AutomatedReorderWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<AutomatedReorderWorker> _logger;

    public AutomatedReorderWorker(
        IServiceProvider services,
        IConfiguration config,
        ILogger<AutomatedReorderWorker> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automated Purchase Order Reorder Daemon initialized.");

        // Initial delay to allow API & DB migration startup
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalHours = _config.GetValue<int>("Procurement:AutoReorderIntervalHours", 6);
            var checkInterval = TimeSpan.FromHours(intervalHours > 0 ? intervalHours : 6);

            try
            {
                _logger.LogInformation("Executing automated stock reorder evaluation cycle...");

                await using var scope = _services.CreateAsyncScope();
                var poService = scope.ServiceProvider.GetRequiredService<IPurchaseOrderService>();
                var notificationService = scope.ServiceProvider.GetService<IRealTimeNotificationService>();

                var result = await poService.ExecuteAutomatedReorderAsync(null, stoppingToken);

                if (result.OrdersCreatedCount > 0 || result.OrdersUpdatedCount > 0)
                {
                    _logger.LogInformation(
                        "Auto-Reorder cycle completed: {Created} POs created, {Updated} POs updated across {Depleted} depleted items (Estimated Valuation: {Valuation:N0} XAF).",
                        result.OrdersCreatedCount,
                        result.OrdersUpdatedCount,
                        result.DepletedItemsDetected,
                        result.TotalEstimatedValuationXaf);

                    if (notificationService != null)
                    {
                        await notificationService.BroadcastNotificationAsync(new StoreNotificationDto
                        {
                            Title = "Automated Replenishment POs Drafted",
                            Message = $"Created {result.OrdersCreatedCount} and updated {result.OrdersUpdatedCount} purchase orders for {result.DepletedItemsDetected} depleted items.",
                            Category = NotificationCategory.PurchaseOrder,
                            Severity = "Info",
                            TargetUrl = "/PurchaseOrders",
                            ActionLabel = "Review Orders"
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("Auto-Reorder cycle evaluated: All items are adequately stocked above threshold.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during automated reorder evaluation cycle.");
            }

            try
            {
                await Task.Delay(checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Automated Purchase Order Reorder Daemon stopped.");
    }
}
