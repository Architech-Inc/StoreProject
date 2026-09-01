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
}
