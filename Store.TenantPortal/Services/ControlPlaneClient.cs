using System.Net.Http.Json;
using Store.TenantPortal.Models.DTOs;
// MT-02 — alias so the return type stays close to its origin while still
// being visible to portal callers.
using CreateInvoiceResponse = Store.Models.DTOs.Payments.CreateInvoiceResponse;

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
                $"api/control/tenants/check-slug?slug={Uri.EscapeDataString(slug)}", ct);

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

    public async Task<PortalAuthDto?> GetAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<PortalAuthDto>>($"api/control/auth/account/{accountId}", ct);
            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching portal account {AccountId}", accountId);
            return null;
        }
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

    // MT-01 — async provisioning: submit + poll for status.
    public async Task<ProvisioningJobResponse> ProvisionTenantAsyncJobAsync(
        ProvisionTenantDto request, Guid accountId, CancellationToken ct = default)
    {
        var body = new
        {
            StoreName = request.StoreName,
            Slug = request.Slug,
            AdminEmail = request.AdminEmail,
            AdminUsername = request.AdminUsername,
            AdminPassword = request.AdminPassword,
            Currency = request.Currency,
            PlanTier = request.PlanTier,
            CustomDomain = request.CustomDomain,
            AccountId = accountId
        };
        var response = await _http.PostAsJsonAsync("api/control/tenants/provision-async", body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Tenant provisioning queue failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProvisioningJobResponse>>(cancellationToken: ct);
        return result!.Data;
    }

    public async Task<ProvisioningJobResponse?> GetProvisioningJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/control/tenants/provisioning/{jobId}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProvisioningJobResponse>>(cancellationToken: ct);
        return result?.Data;
    }

    public async Task<bool> RetryProvisioningJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/tenants/provisioning/{jobId}/retry", null, ct);
        return response.IsSuccessStatusCode;
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

    // ==========================================
    // Phase 4: Audit Trail
    // ==========================================

    public async Task<IReadOnlyList<TenantAuditDto>> GetAuditTrailAsync(Guid tenantId, int limit = 50, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<TenantAuditDto>>>(
                $"api/control/tenants/{tenantId}/audit?limit={limit}", ct);
            return response?.Data ?? Array.Empty<TenantAuditDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit trail for {TenantId}", tenantId);
            return Array.Empty<TenantAuditDto>();
        }
    }

    // ==========================================
    // Phase 5: SDLC & Sandboxing
    // ==========================================

    public async Task<TenantSdlcStatusDto?> GetSdlcStatusAsync(string slug, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<TenantSdlcStatusDto>($"api/control/sdlc/tenants/{slug}/status", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SDLC status for tenant {Slug}", slug);
            return null;
        }
    }

    public async Task<IReadOnlyList<SystemReleaseDto>> GetReleasesAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<List<SystemReleaseDto>>("api/control/sdlc/releases", ct);
            return res ?? new List<SystemReleaseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system releases");
            return Array.Empty<SystemReleaseDto>();
        }
    }

    public async Task<IReadOnlyList<TenantSnapshotDto>> GetSnapshotsAsync(string slug, CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<List<TenantSnapshotDto>>($"api/control/sdlc/tenants/{slug}/snapshots", ct);
            return res ?? new List<TenantSnapshotDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshots for tenant {Slug}", slug);
            return Array.Empty<TenantSnapshotDto>();
        }
    }

    public async Task<bool> UpgradeTenantAsync(string slug, Guid releaseId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/sdlc/tenants/{slug}/upgrade/{releaseId}", null, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RollbackTenantAsync(string slug, Guid snapshotId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/sdlc/tenants/{slug}/rollback/{snapshotId}", null, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<TenantSummaryDto?> CreateSandboxAsync(string slug, Guid releaseId, bool maskData = true, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/sdlc/tenants/{slug}/sandbox/{releaseId}?maskData={maskData}", null, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TenantSummaryDto>(cancellationToken: ct);
    }

    public async Task<bool> DeleteSandboxAsync(string slug, string sandboxSlug, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/control/sdlc/tenants/{slug}/sandbox/{sandboxSlug}", ct);
        return response.IsSuccessStatusCode;
    }

    // ─── MT-07 — public tenant status + maintenance windows ─────────────────

    /// <summary>
    /// MT-07 — anonymous lookup of tenant status + maintenance schedule.
    /// Used by the public <c>/Status/{slug}</c> page. No authentication
    /// header required; the endpoint is on the public route prefix.
    /// </summary>
    public async Task<TenantStatusDto?> GetTenantPublicStatusAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;

        try
        {
            var response = await _http.GetAsync($"api/public/tenants/{Uri.EscapeDataString(slug)}/status", ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<TenantStatusDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Public status lookup failed for slug {Slug}", slug);
            return null;
        }
    }

    public async Task<MaintenanceWindowDto> AddMaintenanceWindowAsync(Guid tenantId, CreateMaintenanceWindowRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/control/tenants/{tenantId}/maintenance-windows", request, ct);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<MaintenanceWindowDto>(cancellationToken: ct);
        return dto ?? throw new InvalidOperationException("Empty response when scheduling maintenance window.");
    }

    public async Task<bool> RemoveMaintenanceWindowAsync(Guid tenantId, Guid windowId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/control/tenants/{tenantId}/maintenance-windows/{windowId}", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResolveMaintenanceWindowAsync(Guid tenantId, Guid windowId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/control/tenants/{tenantId}/maintenance-windows/{windowId}/resolve", null, ct);
        return response.IsSuccessStatusCode;
    }

    // ─── MT-02 — billing surface ────────────────────────────────────────────

    public async Task<TenantDetailDto?> GetTenantAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        try
        {
            var response = await _http.GetAsync($"api/control/tenants/{Uri.EscapeDataString(slug)}", ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TenantDetailDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetTenant({Slug}) failed.", slug);
            return null;
        }
    }

    public async Task<CreateInvoiceResponse?> CreateBillingInvoiceAsync(string slug, CreateBillingInvoiceRequest request, CancellationToken ct = default)
    {
        // MT-02 — ControlPlane proxies PayDunya. The portal hands the
        // invoice request off and gets back the hosted-checkout URL.
        var response = await _http.PostAsJsonAsync($"api/billing/paydunya/invoice", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("CreateBillingInvoice failed for {Slug}: {Status}", slug, response.StatusCode);
            return null;
        }
        var dto = await response.Content.ReadFromJsonAsync<CreateInvoiceResponse>(cancellationToken: ct);
        return dto;
    }

    public async Task<CreateInvoiceResponse?> CreateFlutterwaveBillingInvoiceAsync(string slug, CreateBillingInvoiceRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/billing/flutterwave/invoice", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("CreateFlutterwaveBillingInvoice failed for {Slug}: {Status}", slug, response.StatusCode);
            return null;
        }
        var dto = await response.Content.ReadFromJsonAsync<CreateInvoiceResponse>(cancellationToken: ct);
        return dto;
    }

    public async Task<TenantPaymentHistoryDto?> GetBillingHistoryAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        try
        {
            var response = await _http.GetAsync($"api/billing/paydunya/payments/{Uri.EscapeDataString(slug)}", ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TenantPaymentHistoryDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetBillingHistory({Slug}) failed.", slug);
            return null;
        }
    }

    // ==========================================
    // MT-04: Tenant Custom SMTP Relay
    // ==========================================

    public async Task<Store.Models.DTOs.Tenant.TenantSmtpConfigDto?> GetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/control/tenants/{tenantId}/smtp", ct);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Store.Models.DTOs.Tenant.TenantSmtpConfigDto>>(cancellationToken: ct);
            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch SMTP config for tenant {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<Store.Models.DTOs.Tenant.TenantSmtpConfigDto> UpdateSmtpConfigAsync(Guid tenantId, Store.Models.DTOs.Tenant.UpdateTenantSmtpRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/control/tenants/{tenantId}/smtp", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            throw new InvalidOperationException(err?.Message ?? "Failed to update SMTP configuration.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Store.Models.DTOs.Tenant.TenantSmtpConfigDto>>(cancellationToken: ct);
        return result!.Data!;
    }

    public async Task<Store.Models.DTOs.Tenant.TestSmtpResponse> TestSmtpConfigAsync(Guid tenantId, Store.Models.DTOs.Tenant.TestSmtpRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/control/tenants/{tenantId}/smtp/test", request, ct);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Store.Models.DTOs.Tenant.TestSmtpResponse>>(cancellationToken: ct);
        if (result?.Data != null)
        {
            return result.Data;
        }
        return new Store.Models.DTOs.Tenant.TestSmtpResponse(false, result?.Message ?? "SMTP connection test failed.", 0);
    }

    public async Task<bool> ResetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/control/tenants/{tenantId}/smtp", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset SMTP config for tenant {TenantId}", tenantId);
            return false;
        }
    }
}

