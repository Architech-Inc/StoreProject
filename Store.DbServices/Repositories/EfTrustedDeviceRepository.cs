using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Repositories;

/// <summary>
/// SEC-23 — EF Core implementation of <see cref="ITrustedDeviceRepository"/>.
/// Thin pass-through over <see cref="StoreDbContext.TrustedDevices"/>; the
/// logic (deciding whether to insert / update) lives in
/// <see cref="Services.TrustedDeviceService"/> so the service stays the
/// single source of truth for the device-state machine.
/// </summary>
public class EfTrustedDeviceRepository : ITrustedDeviceRepository
{
    private readonly StoreDbContext _db;

    public EfTrustedDeviceRepository(StoreDbContext db)
    {
        _db = db;
    }

    public Task<TrustedDevice?> FindByFingerprintAsync(Guid userId, string fingerprintHash, CancellationToken ct = default)
        => _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.FingerprintHash == fingerprintHash && !d.IsRevoked, ct);

    public async Task<IReadOnlyList<TrustedDevice>> ListAsync(Guid userId, CancellationToken ct = default)
        => await _db.TrustedDevices
            .Where(d => d.UserId == userId && !d.IsRevoked)
            .OrderByDescending(d => d.LastSeenAtUtc)
            .ToListAsync(ct);

    public Task<TrustedDevice?> FindByPublicIdAsync(Guid userId, string deviceId, CancellationToken ct = default)
        => _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId, ct);

    public async Task<TrustedDevice> UpsertAsync(TrustedDevice device, CancellationToken ct = default)
    {
        if (device.TrustedDeviceId == 0)
        {
            await _db.TrustedDevices.AddAsync(device, ct);
        }
        else
        {
            _db.TrustedDevices.Update(device);
        }
        await _db.SaveChangesAsync(ct);
        return device;
    }

    public Task<TrustedDevice?> FindByIdAsync(int trustedDeviceId, CancellationToken ct = default)
        => _db.TrustedDevices.FirstOrDefaultAsync(d => d.TrustedDeviceId == trustedDeviceId, ct);

    public async Task<bool> RevokeAsync(Guid userId, string deviceId, CancellationToken ct = default)
    {
        var row = await _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId && !d.IsRevoked, ct);
        if (row is null) return false;

        row.IsRevoked = true;
        row.LastModified = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetTrustedAsync(Guid userId, string deviceId, DateTime untilUtc, CancellationToken ct = default)
    {
        var row = await _db.TrustedDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId && !d.IsRevoked, ct);
        if (row is null) return false;

        row.IsTrusted = true;
        row.TrustedUntilUtc = untilUtc;
        row.LastModified = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
