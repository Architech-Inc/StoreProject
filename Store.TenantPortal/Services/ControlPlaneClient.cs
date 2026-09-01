using System.Net.Http.Json;
using Store.TenantPortal.Models.DTOs;

namespace Store.TenantPortal.Services;

public class ControlPlaneClient : IControlPlaneClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ControlPlaneClient> _logger;

    public ControlPlaneClient(HttpClient http, ILogger<ControlPlaneClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SlugCheckDto> CheckSlugAvailabilityAsync(string slug, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SlugCheckDto>>(
                $"api/control/slugs/check?slug={Uri.EscapeDataString(slug)}", ct);

            return response?.Data ?? new SlugCheckDto(slug, false, "Unable to check slug availability.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking slug availability for {Slug}", slug);
            return new SlugCheckDto(slug, false, "Error communicating with Control Plane.");
        }
    }

    public async Task<PortalAuthDto> RegisterAccountAsync(string email, string fullName, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/control/auth/register", new
        {
            Email = email,
            FullName = fullName,
            Password = password
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Registration failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PortalAuthDto>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<PortalAuthDto?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/control/auth/login", new
        {
            Email = email,
            Password = password
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PortalAuthDto>>(cancellationToken: ct);
        return result?.Data;
    }

    public async Task<TenantSummaryDto> ProvisionTenantAsync(ProvisionTenantDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/control/tenants/provision", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Tenant provisioning failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TenantSummaryDto>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<TenantDetailDto?> GetTenantDetailsAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<TenantDetailDto>>($"api/control/tenants/{tenantId}", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant details for {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> CheckTenantHealthAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsync($"api/control/tenants/{tenantId}/health", null, ct);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: ct);
            return result?.Data ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tenant health for {TenantId}", tenantId);
            return false;
        }
    }

    public async Task LinkAccountToTenantAsync(Guid accountId, Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            await _http.PostAsJsonAsync("api/control/auth/link-tenant", new { AccountId = accountId, TenantId = tenantId }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking account {AccountId} to tenant {TenantId}", accountId, tenantId);
        }
    }

    // ==========================================
    // Phase 2: Environment Control
    // ==========================================

    public async Task<EnvironmentStatusDto?> GetEnvironmentStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<EnvironmentStatusDto>>(
                $"api/control/tenants/{tenantId}/environment", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting environment status for {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> RestartServiceAsync(Guid tenantId, string serviceName, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsync(
                $"api/control/tenants/{tenantId}/environment/restart/{Uri.EscapeDataString(serviceName)}", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting service {ServiceName} for {TenantId}", serviceName, tenantId);
            return false;
        }
    }

    public async Task<bool> SuspendTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsync($"api/control/tenants/{tenantId}/environment/suspend", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> ResumeTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsync($"api/control/tenants/{tenantId}/environment/resume", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming tenant {TenantId}", tenantId);
            return false;
        }
    }

    // ==========================================
    // Phase 2: Custom Domains
    // ==========================================

    public async Task<TenantDomainDto?> GetDomainConfigAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<TenantDomainDto>>(
                $"api/control/tenants/{tenantId}/domains", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting domain config for {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<TenantDomainDto> SetCustomDomainAsync(Guid tenantId, string domain, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"api/control/tenants/{tenantId}/domains/custom", new SetCustomDomainRequest(domain), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to set custom domain.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TenantDomainDto>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<VerifyDomainResponse> VerifyCustomDomainAsync(Guid tenantId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/tenants/{tenantId}/domains/verify", null, ct);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VerifyDomainResponse>>(cancellationToken: ct);
        return result?.Data ?? new VerifyDomainResponse("", false, "Failed", null, null, null, "Verification request failed.");
    }

    public async Task<bool> RemoveCustomDomainAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/control/tenants/{tenantId}/domains/custom", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing custom domain for {TenantId}", tenantId);
            return false;
        }
    }

    // ==========================================
    // Phase 2: Branch Subdomains
    // ==========================================

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<BranchDto>>>(
                $"api/control/tenants/{tenantId}/branches", ct);
            return response?.Data ?? Array.Empty<BranchDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branches for {TenantId}", tenantId);
            return Array.Empty<BranchDto>();
        }
    }

    public async Task<BranchDto> AddBranchAsync(Guid tenantId, CreateBranchRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/control/tenants/{tenantId}/branches", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to add branch.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BranchDto>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<VerifyDomainResponse> VerifyBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/tenants/{tenantId}/branches/{branchId}/verify", null, ct);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VerifyDomainResponse>>(cancellationToken: ct);
        return result?.Data ?? new VerifyDomainResponse("", false, "Failed", null, null, null, "Branch verification failed.");
    }

    public async Task<bool> RemoveBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/control/tenants/{tenantId}/branches/{branchId}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing branch {BranchId} for {TenantId}", branchId, tenantId);
            return false;
        }
    }

    // ==========================================
    // Phase 3: Cloud Backups & Storage
    // ==========================================

    public async Task<BackupSummaryDto?> GetBackupSummaryAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<BackupSummaryDto>>(
                $"api/control/tenants/{tenantId}/backups", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backup summary for {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<TriggerBackupResponse> TriggerBackupAsync(Guid tenantId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/tenants/{tenantId}/backups/trigger", null, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to trigger backup.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TriggerBackupResponse>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<BackupProviderDto> ConfigureS3ProviderAsync(Guid tenantId, ConfigureS3Request request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/control/tenants/{tenantId}/backups/providers/s3", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to configure S3 provider.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BackupProviderDto>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<BackupProviderDto> SaveOAuthTokensAsync(Guid tenantId, SaveOAuthTokensRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/control/tenants/{tenantId}/backups/providers/oauth", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to save OAuth tokens.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BackupProviderDto>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<bool> DisconnectBackupProviderAsync(Guid tenantId, string providerType, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/control/tenants/{tenantId}/backups/providers/{providerType}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting backup provider {ProviderType} for {TenantId}", providerType, tenantId);
            return false;
        }
    }

    public async Task<BackupScheduleDto> UpdateBackupScheduleAsync(Guid tenantId, UpdateScheduleRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/control/tenants/{tenantId}/backups/schedule", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to update backup schedule.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<BackupScheduleDto>>(cancellationToken: ct);
        return result!.Data;
    }
}
