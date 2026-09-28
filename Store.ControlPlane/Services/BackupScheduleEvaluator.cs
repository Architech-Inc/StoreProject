using Store.ControlPlane.Models;

namespace Store.ControlPlane.Services;

/// <summary>
/// MT-06 — pure-function cron evaluator for the per-tenant backup schedule.
///
/// Lives outside the hosted service so the next-run logic can be unit-tested
/// deterministically. The hosted service calls <see cref="ShouldRunNow"/> on
/// each tick and <see cref="ComputeNextRunAt"/> after each successful run.
///
/// Rules:
///   - <see cref="BackupFrequency.Manual"/>  : never auto-run.
///   - <see cref="BackupFrequency.Hourly"/>  : next run is `lastRunAt + 1h`.
///   - <see cref="BackupFrequency.Daily"/>   : next run is `lastRunAt + 1d` at 02:00 UTC.
///   - <see cref="BackupFrequency.Weekly"/>  : next run is `lastRunAt + 7d` at 02:00 UTC.
///
/// If <paramref name="lastRunAt"/> is null (first run), we compute from
/// <paramref name="now"/> so the schedule fires promptly after enable.
/// </summary>
public static class BackupScheduleEvaluator
{
    /// <summary>
    /// Returns true when the schedule should fire at <paramref name="now"/>.
    /// Always false for <see cref="BackupFrequency.Manual"/> or when disabled.
    /// </summary>
    public static bool ShouldRunNow(
        BackupScheduleConfig schedule,
        DateTime nowUtc)
    {
        if (schedule is null) return false;
        if (!schedule.IsEnabled) return false;
        if (schedule.Frequency == BackupFrequency.Manual) return false;

        var next = schedule.NextRunAt ?? ComputeNextRunAt(schedule, null, nowUtc);
        return next <= nowUtc;
    }

    /// <summary>
    /// Compute the next run time given the schedule, the previous run (or null
    /// for "first run"), and the current time. Always returns a future UTC time.
    /// </summary>
    public static DateTime ComputeNextRunAt(
        BackupScheduleConfig schedule,
        DateTime? lastRunAtUtc,
        DateTime nowUtc)
    {
        if (schedule.Frequency == BackupFrequency.Manual)
        {
            // Manual schedule — never auto-runs. Return a sentinel far future
            // so the worker can detect "no scheduled time".
            return DateTime.MaxValue;
        }

        var anchor = lastRunAtUtc ?? nowUtc;

        if (schedule.Frequency == BackupFrequency.Daily || schedule.Frequency == BackupFrequency.Weekly)
        {
            // For Daily/Weekly we anchor runs at 02:00 UTC. Find the first
            // 02:00 slot strictly after `anchor` AND strictly after `nowUtc`.
            var candidate = new DateTime(anchor.Year, anchor.Month, anchor.Day, 2, 0, 0, DateTimeKind.Utc);
            while (candidate <= anchor || candidate <= nowUtc)
            {
                candidate = candidate.AddDays(1);
            }
            return candidate;
        }

        // Hourly: simply anchor + 1h. Always in the future when the caller
        // passes lastRunAtUtc = nowUtc (the post-fire path).
        return DateTime.SpecifyKind(anchor, DateTimeKind.Utc) + TimeSpan.FromHours(1);
    }
}