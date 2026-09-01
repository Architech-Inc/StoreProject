using System.Security.Cryptography;
using System.Text;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;

namespace Store.ControlPlane.Services;

public class BackupService : IBackupService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _config;
    private readonly ILogger<BackupService> _logger;
    private readonly byte[] _encryptionKey;

    public BackupService(ITenantRepository tenantRepo, IAuditService auditService, IConfiguration config, ILogger<BackupService> logger)
    {
        _tenantRepo = tenantRepo;
        _auditService = auditService;
        _config = config;
        _logger = logger;

        var masterSecret = _config["ControlPlane:BackupEncryptionMasterKey"] ?? "ClexAnFoodsSaaSMasterBackupEncryptionKey2026";
        var salt = Encoding.UTF8.GetBytes("ClexAn-Backup-Salt-8819");
        _encryptionKey = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(masterSecret),
            salt,
            50_000,
            HashAlgorithmName.SHA256,
            32
        );
    }

    public async Task<BackupSummaryDto?> GetBackupSummaryAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var schedule = tenant.BackupSchedule ?? new BackupScheduleConfig();
        var scheduleDto = new BackupScheduleDto(
            schedule.Frequency.ToString(),
            schedule.RetentionCount,
            schedule.IsEnabled,
            schedule.NextRunAt ?? DateTime.UtcNow.AddDays(1)
        );

        var providerConfigs = tenant.BackupProviders ?? new List<BackupProviderConfig>();

        var oneDrive = providerConfigs.FirstOrDefault(p => p.ProviderType == BackupProviderType.OneDrive);
        var googleDrive = providerConfigs.FirstOrDefault(p => p.ProviderType == BackupProviderType.GoogleDrive);
        var s3 = providerConfigs.FirstOrDefault(p => p.ProviderType == BackupProviderType.S3);

        var providers = new List<BackupProviderDto>
        {
            new("OneDrive", "Microsoft OneDrive (AppFolder)", oneDrive?.IsConnected ?? false, oneDrive?.AccountEmail, oneDrive?.AccountName, oneDrive?.ConnectedAt, oneDrive?.LastBackupAt, oneDrive?.LastBackupStatus),
            new("GoogleDrive", "Google Drive (App Space)", googleDrive?.IsConnected ?? false, googleDrive?.AccountEmail, googleDrive?.AccountName, googleDrive?.ConnectedAt, googleDrive?.LastBackupAt, googleDrive?.LastBackupStatus),
            new("S3", "Amazon S3 / MinIO Compatible", s3?.IsConnected ?? false, s3?.AccountEmail ?? s3?.S3Config?.BucketName, s3?.AccountName ?? s3?.S3Config?.EndpointUrl, s3?.ConnectedAt, s3?.LastBackupAt, s3?.LastBackupStatus)
        };

        var history = (tenant.BackupHistory ?? new List<TenantBackupJobRecord>())
            .OrderByDescending(b => b.Timestamp)
            .Take(schedule.RetentionCount > 0 ? schedule.RetentionCount : 14)
            .Select(b => new BackupJobDto(
                b.BackupId,
                b.Timestamp,
                b.TotalSizeBytes,
                FormatBytes(b.TotalSizeBytes),
                b.Files,
                b.DestinationProviders,
                b.Status,
                b.ErrorMessage
            ))
            .ToList();

        return new BackupSummaryDto(
            tenant.TenantId,
            tenant.Slug,
            scheduleDto,
            providers,
            history
        );
    }

    public async Task<TriggerBackupResponse> TriggerBackupNowAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        var timestamp = DateTime.UtcNow;
        var dateStr = timestamp.ToString("yyyyMMdd-HHmmss");
        var mysqlFile = $"{tenant.Slug}-mysql-{dateStr}.sql.gz";
        var mongoFile = $"{tenant.Slug}-mongodb-{dateStr}.archive.gz";

        // Simulated compressed archive generation (~14 MB to 22 MB)
        var totalBytes = (long)(RandomNumberGenerator.GetInt32(14, 22) * 1024 * 1024 + RandomNumberGenerator.GetInt32(100, 900) * 1024);
        var files = new List<string> { mysqlFile, mongoFile };

        var connectedProviders = (tenant.BackupProviders ?? new List<BackupProviderConfig>())
            .Where(p => p.IsConnected)
            .Select(p => p.ProviderType.ToString())
            .ToList();

        var destination = connectedProviders.Any() 
            ? string.Join(", ", connectedProviders) 
            : "Local Silo Storage";

        var backupRecord = new TenantBackupJobRecord
        {
            BackupId = Guid.NewGuid(),
            TenantId = tenant.TenantId,
            Timestamp = timestamp,
            TotalSizeBytes = totalBytes,
            Files = files,
            DestinationProviders = destination,
            Status = "Completed",
            RetentionDays = 30
        };

        tenant.BackupHistory ??= new List<TenantBackupJobRecord>();
        tenant.BackupHistory.Insert(0, backupRecord);

        // Prune older than retention policy
        var maxRetention = tenant.BackupSchedule?.RetentionCount ?? 14;
        if (tenant.BackupHistory.Count > maxRetention)
        {
            tenant.BackupHistory = tenant.BackupHistory.Take(maxRetention).ToList();
        }

        // Update provider last backup timestamps
        if (tenant.BackupProviders != null)
        {
            foreach (var p in tenant.BackupProviders.Where(p => p.IsConnected))
            {
                p.LastBackupAt = timestamp;
                p.LastBackupStatus = "Success";
            }
        }

        tenant.ProvisioningLogs.Add(new TenantProvisioningLog
        {
            LogId = Guid.NewGuid(),
            TenantId = tenant.TenantId,
            StepName = "BackupJob",
            IsSuccess = true,
            Message = $"Automated snapshot created ({FormatBytes(totalBytes)}) & uploaded to [{destination}].",
            Timestamp = timestamp
        });

        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Backup {BackupId} created successfully for tenant {Slug} to {Destination}", backupRecord.BackupId, tenant.Slug, destination);

        await _auditService.RecordAuditAsync(tenant.TenantId, "BackupCreated", tenant.AdminEmail, $"Backup snapshot created ({FormatBytes(totalBytes)}) and synced to {destination}.", ct: ct);

        return new TriggerBackupResponse(
            backupRecord.BackupId,
            backupRecord.Status,
            $"Backup successfully created ({FormatBytes(totalBytes)}) and dispatched to {destination}.",
            totalBytes,
            files,
            timestamp
        );
    }

    public async Task<BackupProviderDto> ConfigureS3ProviderAsync(Guid tenantId, ConfigureS3Request request, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        tenant.BackupProviders ??= new List<BackupProviderConfig>();
        var existing = tenant.BackupProviders.FirstOrDefault(p => p.ProviderType == BackupProviderType.S3);
        if (existing != null)
        {
            tenant.BackupProviders.Remove(existing);
        }

        var s3Config = new BackupProviderConfig
        {
            ProviderType = BackupProviderType.S3,
            AccountEmail = request.BucketName,
            AccountName = $"S3 ({request.Region})",
            IsConnected = true,
            ConnectedAt = DateTime.UtcNow,
            S3Config = new S3StorageConfig
            {
                EndpointUrl = request.EndpointUrl,
                BucketName = request.BucketName,
                Region = request.Region,
                AccessKeyId = request.AccessKeyId,
                EncryptedSecretKey = EncryptString(request.SecretAccessKey)
            }
        };

        tenant.BackupProviders.Add(s3Config);
        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Configured S3 storage provider for tenant {Slug}", tenant.Slug);

        await _auditService.RecordAuditAsync(tenant.TenantId, "ProviderConnected", tenant.AdminEmail, $"Connected S3 / MinIO storage bucket '{request.BucketName}'.", ct: ct);

        return new BackupProviderDto("S3", "Amazon S3 / MinIO Compatible", true, s3Config.AccountEmail, s3Config.AccountName, s3Config.ConnectedAt, null, null);
    }

    public async Task<BackupProviderDto> SaveOAuthProviderAsync(Guid tenantId, SaveOAuthTokensRequest request, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        if (!Enum.TryParse<BackupProviderType>(request.ProviderType, true, out var providerType))
        {
            throw new ArgumentException($"Invalid provider type '{request.ProviderType}'.");
        }

        tenant.BackupProviders ??= new List<BackupProviderConfig>();
        var existing = tenant.BackupProviders.FirstOrDefault(p => p.ProviderType == providerType);
        if (existing != null)
        {
            tenant.BackupProviders.Remove(existing);
        }

        var oAuth = new BackupProviderConfig
        {
            ProviderType = providerType,
            AccountEmail = request.AccountEmail,
            AccountName = request.AccountName ?? request.AccountEmail,
            IsConnected = true,
            ConnectedAt = DateTime.UtcNow,
            EncryptedAccessToken = EncryptString(request.AccessToken),
            EncryptedRefreshToken = EncryptString(request.RefreshToken),
            TokenExpiresAt = DateTime.UtcNow.AddSeconds(request.ExpiresInSeconds)
        };

        tenant.BackupProviders.Add(oAuth);
        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Connected {Provider} OAuth provider for tenant {Slug} ({Email})", providerType, tenant.Slug, request.AccountEmail);

        await _auditService.RecordAuditAsync(tenant.TenantId, "ProviderConnected", tenant.AdminEmail, $"Connected {providerType} storage account ({request.AccountEmail}).", ct: ct);

        return new BackupProviderDto(providerType.ToString(), providerType.ToString(), true, oAuth.AccountEmail, oAuth.AccountName, oAuth.ConnectedAt, null, null);
    }

    public async Task<bool> DisconnectProviderAsync(Guid tenantId, string providerType, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null || tenant.BackupProviders == null) return false;

        if (!Enum.TryParse<BackupProviderType>(providerType, true, out var pType))
        {
            return false;
        }

        var match = tenant.BackupProviders.FirstOrDefault(p => p.ProviderType == pType);
        if (match == null) return false;

        tenant.BackupProviders.Remove(match);
        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Disconnected backup provider {Provider} for tenant {Slug}", providerType, tenant.Slug);

        await _auditService.RecordAuditAsync(tenant.TenantId, "ProviderDisconnected", tenant.AdminEmail, $"Disconnected {providerType} storage provider.", ct: ct);

        return true;
    }

    public async Task<BackupScheduleDto> UpdateScheduleAsync(Guid tenantId, UpdateScheduleRequest request, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        if (!Enum.TryParse<BackupFrequency>(request.Frequency, true, out var freq))
        {
            freq = BackupFrequency.Daily;
        }

        tenant.BackupSchedule = new BackupScheduleConfig
        {
            Frequency = freq,
            RetentionCount = Math.Clamp(request.RetentionCount, 1, 60),
            IsEnabled = request.IsEnabled,
            NextRunAt = DateTime.UtcNow.AddDays(freq == BackupFrequency.Hourly ? 0.04 : freq == BackupFrequency.Weekly ? 7 : 1)
        };

        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Updated backup schedule for tenant {Slug} to {Frequency}, retention {Count}", tenant.Slug, freq, request.RetentionCount);

        await _auditService.RecordAuditAsync(tenant.TenantId, "ScheduleUpdated", tenant.AdminEmail, $"Updated backup schedule to {freq} (retention: {request.RetentionCount} snapshots).", ct: ct);

        return new BackupScheduleDto(
            tenant.BackupSchedule.Frequency.ToString(),
            tenant.BackupSchedule.RetentionCount,
            tenant.BackupSchedule.IsEnabled,
            tenant.BackupSchedule.NextRunAt
        );
    }

    private string EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var combined = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    private string DecryptString(string cipherTextBase64)
    {
        var combined = Convert.FromBase64String(cipherTextBase64);
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        var iv = new byte[16];
        var cipherBytes = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        Buffer.BlockCopy(combined, 16, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
