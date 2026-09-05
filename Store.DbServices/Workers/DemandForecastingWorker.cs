using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Store.DbServices.Services;

namespace Store.DbServices.Workers;

public class DemandForecastingWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<DemandForecastingWorker> _logger;

    public DemandForecastingWorker(
        IServiceProvider services,
        IConfiguration config,
        ILogger<DemandForecastingWorker> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Demand Forecasting Worker initialized.");

        // Initial delay
        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalHours = _config.GetValue<int>("Forecasting:IntervalHours", 24);
            var checkInterval = TimeSpan.FromHours(intervalHours > 0 ? intervalHours : 24);

            try
            {
                _logger.LogInformation("Executing demand forecasting cycle...");

                await using var scope = _services.CreateAsyncScope();
                var forecastingService = scope.ServiceProvider.GetRequiredService<IDemandForecastingService>();

                await forecastingService.RunDemandForecastingAsync(stoppingToken);

                _logger.LogInformation("Demand forecasting cycle completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during demand forecasting cycle.");
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

        _logger.LogInformation("Demand Forecasting Worker stopped.");
    }
}
