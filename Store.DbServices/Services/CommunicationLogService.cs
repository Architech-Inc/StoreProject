using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class CommunicationLogService : ICommunicationLogService
{
    private readonly IMongoCollection<CommunicationLog> _logsCollection;
    private readonly ILogger<CommunicationLogService> _logger;
    private readonly string _offlineFilePath = "offline_logs.json";

    public CommunicationLogService(IConfiguration config, ILogger<CommunicationLogService> logger)
    {
        _logger = logger;
        
        var connectionString = config["MongoDB:ConnectionString"];
        var databaseName = config["MongoDB:DatabaseName"] ?? "StoreLogsDb";
        
        try
        {
            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseName);
            _logsCollection = mongoDatabase.GetCollection<CommunicationLog>("CommunicationLogs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB client. Offline mode will be used.");
            _logsCollection = null!; // It will be null if connection string is missing or invalid format
        }
    }

    public async Task LogCommunicationAsync(CommunicationLog log, CancellationToken ct = default)
    {
        try
        {
            if (_logsCollection == null)
            {
                throw new InvalidOperationException("MongoDB is not configured.");
            }

            await _logsCollection.InsertOneAsync(log, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log to MongoDB. Falling back to offline JSON file.");
            await LogOfflineAsync(log, ct);
        }
    }

    public async Task<List<CommunicationLog>> GetLogsAsync(int page = 1, int pageSize = 50, string? channel = null, string? status = null, CancellationToken ct = default)
    {
        if (_logsCollection == null) return new List<CommunicationLog>();

        var filter = Builders<CommunicationLog>.Filter.Empty;
        
        if (!string.IsNullOrWhiteSpace(channel) && Enum.TryParse<Store.Models.Enums.CommunicationChannel>(channel, true, out var c))
        {
            filter &= Builders<CommunicationLog>.Filter.Eq(x => x.Channel, c);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Store.Models.Enums.CommunicationStatus>(status, true, out var s))
        {
            filter &= Builders<CommunicationLog>.Filter.Eq(x => x.Status, s);
        }

        return await _logsCollection.Find(filter)
            .SortByDescending(x => x.DateCreated)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    public async Task<long> GetLogsCountAsync(string? channel = null, string? status = null, CancellationToken ct = default)
    {
         if (_logsCollection == null) return 0;

        var filter = Builders<CommunicationLog>.Filter.Empty;
        
        if (!string.IsNullOrWhiteSpace(channel) && Enum.TryParse<Store.Models.Enums.CommunicationChannel>(channel, true, out var c))
        {
            filter &= Builders<CommunicationLog>.Filter.Eq(x => x.Channel, c);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Store.Models.Enums.CommunicationStatus>(status, true, out var s))
        {
            filter &= Builders<CommunicationLog>.Filter.Eq(x => x.Status, s);
        }

        return await _logsCollection.CountDocumentsAsync(filter, cancellationToken: ct);
    }

    private async Task LogOfflineAsync(CommunicationLog log, CancellationToken ct)
    {
        var logs = new List<CommunicationLog>();
        if (File.Exists(_offlineFilePath))
        {
            try
            {
                var existingContent = await File.ReadAllTextAsync(_offlineFilePath, ct);
                if (!string.IsNullOrWhiteSpace(existingContent))
                {
                    logs = JsonSerializer.Deserialize<List<CommunicationLog>>(existingContent) ?? new List<CommunicationLog>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read offline logs file.");
            }
        }

        logs.Add(log);

        try
        {
            var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_offlineFilePath, json, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write to offline logs file.");
        }
    }
}
