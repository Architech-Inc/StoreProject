namespace Store.Models.Interfaces.Services;

public interface ISystemSettingService
{
    Task<string?> GetSettingAsync(string key, CancellationToken ct = default);
    Task<bool> UpdateSettingAsync(string key, string value, CancellationToken ct = default);
}
