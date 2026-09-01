using System.ComponentModel.DataAnnotations;

namespace Store.ControlPlane.Models.DTOs;

public record BackupSummaryDto(
    Guid TenantId,
    string Slug,
    BackupScheduleDto Schedule,
    List<BackupProviderDto> Providers,
    List<BackupJobDto> RecentBackups
);

public record BackupProviderDto(
    string ProviderType, // OneDrive, GoogleDrive, S3, Local
    string DisplayName,
    bool IsConnected,
    string? AccountEmail,
    string? AccountName,
    DateTime? ConnectedAt,
    DateTime? LastBackupAt,
    string? LastBackupStatus
);

public record BackupScheduleDto(
    string Frequency, // Manual, Hourly, Daily, Weekly
    int RetentionCount,
    bool IsEnabled,
    DateTime? NextRunAt
);

public record BackupJobDto(
    Guid BackupId,
    DateTime Timestamp,
    long TotalSizeBytes,
    string FormattedSize,
    List<string> Files,
    string DestinationProviders,
    string Status,
    string? ErrorMessage
);

public record ConfigureS3Request(
    [Required] string EndpointUrl,
    [Required] string BucketName,
    [Required] string Region,
    [Required] string AccessKeyId,
    [Required] string SecretAccessKey
);

public record SaveOAuthTokensRequest(
    [Required] string ProviderType, // OneDrive or GoogleDrive
    [Required] string AccessToken,
    [Required] string RefreshToken,
    [Required] string AccountEmail,
    string? AccountName,
    int ExpiresInSeconds = 3600
);

public record UpdateScheduleRequest(
    string Frequency,
    int RetentionCount,
    bool IsEnabled
);

public record TriggerBackupResponse(
    Guid BackupId,
    string Status,
    string Message,
    long TotalSizeBytes,
    List<string> Files,
    DateTime Timestamp
);
