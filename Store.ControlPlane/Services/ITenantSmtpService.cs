using Store.Models.DTOs.Tenant;

namespace Store.ControlPlane.Services;

/// <summary>
/// MT-04 — Service contract for managing per-tenant custom SMTP relays, rotation, and live test verifications.
/// </summary>
public interface ITenantSmtpService
{
    Task<TenantSmtpConfigDto?> GetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantSmtpConfigDto> UpdateSmtpConfigAsync(Guid tenantId, UpdateTenantSmtpRequest request, CancellationToken ct = default);
    Task<TestSmtpResponse> TestSmtpConnectionAsync(Guid tenantId, TestSmtpRequest request, CancellationToken ct = default);
    Task<bool> ResetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default);
}
