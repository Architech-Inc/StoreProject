using Store.ControlPlane.Models.DTOs;

namespace Store.ControlPlane.Services;

public interface IBackupService
{
    Task<BackupSummaryDto?> GetBackupSummaryAsync(Guid tenantId, CancellationToken ct = default);
    Task<TriggerBackupResponse> TriggerBackupNowAsync(Guid tenantId, CancellationToken ct = default);
    Task<BackupProviderDto> ConfigureS3ProviderAsync(Guid tenantId, ConfigureS3Request request, CancellationToken ct = default);
    Task<BackupProviderDto> SaveOAuthProviderAsync(Guid tenantId, SaveOAuthTokensRequest request, CancellationToken ct = default);
    Task<bool> DisconnectProviderAsync(Guid tenantId, string providerType, CancellationToken ct = default);
    Task<BackupScheduleDto> UpdateScheduleAsync(Guid tenantId, UpdateScheduleRequest request, CancellationToken ct = default);
}
