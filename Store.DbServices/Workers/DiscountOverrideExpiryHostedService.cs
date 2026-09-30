using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Store.DbServices.Context;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Workers;

/// <summary>
/// Wave 23.C — background worker that demotes stale <c>Approved</c>
/// discount override requests to <c>Expired</c> after a TTL (default 15
/// minutes). Keeps the table small and prevents long-lived approval
/// windows from leaking beyond a single POS session.
///
/// Polls every 60s; the SQL itself is cheap (one indexed lookup +
/// a few row updates). Logs the expired count per tick.
/// </summary>
public class DiscountOverrideExpiryHostedService : BackgroundService
{
    /// <summary>TTL for an Approved override before it expires.</summary>
    public static readonly TimeSpan ApprovedTtl = TimeSpan.FromMinutes(15);

    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceProvider _services;
    private readonly ILogger<DiscountOverrideExpiryHostedService> _logger;

    public DiscountOverrideExpiryHostedService(IServiceProvider services, ILogger<DiscountOverrideExpiryHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Discount Override Expiry Sweeper started (ttl = {TtlMinutes}min, tick = {TickSeconds}s).",
            ApprovedTtl.TotalMinutes, TickInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireStaleOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Discount override expiry sweep failed; will retry next tick.");
            }

            try
            {
                await Task.Delay(TickInterval, stoppingToken);
            }
            catch (OperationCanceledException) { /* shutdown */ }
        }
    }

    private async Task ExpireStaleOnceAsync(CancellationToken ct)
    {
        // Build a scope so the DbContext lifetime is per-tick (DbContext is
        // scoped, BackgroundService is singleton).
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

        var cutoff = DateTime.UtcNow - ApprovedTtl;

        var stale = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            db.DiscountOverrideRequests
                .Where(r => r.Status == DiscountOverrideStatus.Approved
                         && r.LastModified < cutoff),
            ct);

        if (stale.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var row in stale)
        {
            row.Status = DiscountOverrideStatus.Expired;
            row.LastModified = now;
        }
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Expired {Count} stale Approved override request(s) older than {TtlMinutes}min.",
            stale.Count, ApprovedTtl.TotalMinutes);
    }
}
