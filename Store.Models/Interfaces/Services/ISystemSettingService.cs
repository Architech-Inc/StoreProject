namespace Store.Models.Interfaces.Services;

/// <summary>
/// Result of an attempt to update a system setting. <see cref="Success"/> tells
/// the caller whether the write landed; <see cref="FailureReason"/> carries
/// a human-readable reason when it didn't, so the API can surface a useful
/// error message instead of a generic "Failed to update setting."
/// </summary>
public record SystemSettingUpdateResult(bool Success, string? FailureReason)
{
    public static SystemSettingUpdateResult Ok() => new(true, null);
    public static SystemSettingUpdateResult Failed(string reason) => new(false, reason);
}

public interface ISystemSettingService
{
    Task<string?> GetSettingAsync(string key, CancellationToken ct = default);
    Task<SystemSettingUpdateResult> UpdateSettingAsync(string key, string value, CancellationToken ct = default);
}
