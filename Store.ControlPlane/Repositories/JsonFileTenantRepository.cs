using System.Text.Json;
using Store.ControlPlane.Models;

namespace Store.ControlPlane.Repositories;

public class JsonFileTenantRepository : ITenantRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public JsonFileTenantRepository(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _filePath = Path.Combine(dataDir, "tenants.json");
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return await LoadTenantsUnsafeAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenants = await GetAllAsync(ct);
        return tenants.FirstOrDefault(t => t.TenantId == tenantId);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var tenants = await GetAllAsync(ct);
        return tenants.FirstOrDefault(t => string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
    {
        var tenants = await GetAllAsync(ct);
        return tenants.Any(t => string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SaveAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var list = (await LoadTenantsUnsafeAsync(ct)).ToList();
            var index = list.FindIndex(t => t.TenantId == tenant.TenantId);
            if (index >= 0)
            {
                list[index] = tenant;
            }
            else
            {
                list.Add(tenant);
            }

            await SaveTenantsUnsafeAsync(list, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(Guid tenantId, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var list = (await LoadTenantsUnsafeAsync(ct)).ToList();
            list.RemoveAll(t => t.TenantId == tenantId);
            await SaveTenantsUnsafeAsync(list, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Tenant>> LoadTenantsUnsafeAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath)) return new List<Tenant>();
        var json = await File.ReadAllTextAsync(_filePath, ct);
        if (string.IsNullOrWhiteSpace(json)) return new List<Tenant>();
        return JsonSerializer.Deserialize<List<Tenant>>(json, _jsonOptions) ?? new List<Tenant>();
    }

    private async Task SaveTenantsUnsafeAsync(List<Tenant> tenants, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(tenants, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }
}
