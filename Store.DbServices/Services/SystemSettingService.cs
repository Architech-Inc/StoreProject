using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly IUnitOfWork _uow;

    public SystemSettingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<string?> GetSettingAsync(string key, CancellationToken ct = default)
    {
        var setting = await _uow.Repository<SystemSetting>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == key, ct);

        return setting?.SettingValue;
    }

    public async Task<bool> UpdateSettingAsync(string key, string value, CancellationToken ct = default)
    {
        var setting = await _uow.Repository<SystemSetting>().Query()
            .FirstOrDefaultAsync(s => s.SettingKey == key, ct);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                SettingKey = key,
                SettingValue = value,
                LastModified = DateTime.UtcNow
            };
            await _uow.Repository<SystemSetting>().AddAsync(setting, ct);
        }
        else
        {
            setting.SettingValue = value;
            setting.LastModified = DateTime.UtcNow;
            _uow.Repository<SystemSetting>().Update(setting);
        }

        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
