using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Store.Models.Entities;

namespace Store.DbServices.Workers;

public class OfflineLogSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OfflineLogSyncWorker> _logger;
    private readonly string _offlineFilePath = "offline_logs.json";
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2); // Check every 2 minutes

    public OfflineLogSyncWorker(IServiceProvider serviceProvider, ILogger<OfflineLogSyncWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OfflineLogSyncWorker started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(_offlineFilePath))
                {
                    await ProcessOfflineLogsAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing OfflineLogSyncWorker.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
        
        _logger.LogInformation("OfflineLogSyncWorker stopping.");
    }

    private async Task ProcessOfflineLogsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var connectionString = config["MongoDB:ConnectionString"];
        var databaseName = config["MongoDB:DatabaseName"] ?? "StoreLogsDb";
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Can't sync if MongoDB isn't configured
            return;
        }

        IMongoCollection<CommunicationLog> logsCollection;
        try
        {
            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseName);
            logsCollection = mongoDatabase.GetCollection<CommunicationLog>("CommunicationLogs");
            
            // Ping to verify connection
            await mongoDatabase.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}", cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("MongoDB is not reachable. Skipping sync. Error: {Message}", ex.Message);
            return; // Exit if offline
        }

        List<CommunicationLog> offlineLogs = new();
        try
        {
            var json = await File.ReadAllTextAsync(_offlineFilePath, stoppingToken);
            if (!string.IsNullOrWhiteSpace(json))
            {
                offlineLogs = JsonSerializer.Deserialize<List<CommunicationLog>>(json) ?? new List<CommunicationLog>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read offline logs file during sync.");
            return;
        }

        if (offlineLogs.Any())
        {
            _logger.LogInformation("Found {Count} offline logs to sync.", offlineLogs.Count);
            try
            {
                // Reset IDs so MongoDB generates new ones if they were saved with empty/null strings
                foreach (var log in offlineLogs)
                {
                    log.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                }

                await logsCollection.InsertManyAsync(offlineLogs, cancellationToken: stoppingToken);
                _logger.LogInformation("Successfully synced {Count} offline logs to MongoDB.", offlineLogs.Count);
                
                // Clear the file
                await File.WriteAllTextAsync(_offlineFilePath, "[]", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert offline logs into MongoDB.");
            }
        }
    }
}
