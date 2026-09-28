using Store.ControlPlane.Models;
using Store.ControlPlane.Services;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// MT-06 — pure-function unit tests for the per-tenant backup cron
/// evaluator. Cover every transition: hourly/daily/weekly next-run, manual
/// never-fires, disabled never-fires, future NextRunAt defers, past
/// NextRunAt fires, past-anchored Daily/Weekly snap to 02:00 UTC, and
/// never-schedule-in-the-past guarantee.
/// </summary>
public class BackupScheduleEvaluatorTests
{
    private static readonly DateTime Now = new(2026, 9, 17, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Manual_never_fires()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Manual, IsEnabled = true };
        Assert.False(BackupScheduleEvaluator.ShouldRunNow(schedule, Now));
    }

    [Fact]
    public void Disabled_never_fires()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Daily, IsEnabled = false };
        Assert.False(BackupScheduleEvaluator.ShouldRunNow(schedule, Now));
    }

    [Fact]
    public void Hourly_fires_when_NextRunAt_in_past()
    {
        var schedule = new BackupScheduleConfig
        {
            Frequency = BackupFrequency.Hourly,
            IsEnabled = true,
            NextRunAt = Now.AddMinutes(-1)
        };
        Assert.True(BackupScheduleEvaluator.ShouldRunNow(schedule, Now));
    }

    [Fact]
    public void Daily_defers_when_NextRunAt_in_future()
    {
        var schedule = new BackupScheduleConfig
        {
            Frequency = BackupFrequency.Daily,
            IsEnabled = true,
            NextRunAt = Now.AddHours(6)
        };
        Assert.False(BackupScheduleEvaluator.ShouldRunNow(schedule, Now));
    }

    [Fact]
    public void Weekly_fires_when_NextRunAt_equals_now()
    {
        var schedule = new BackupScheduleConfig
        {
            Frequency = BackupFrequency.Weekly,
            IsEnabled = true,
            NextRunAt = Now
        };
        Assert.True(BackupScheduleEvaluator.ShouldRunNow(schedule, Now));
    }

    [Fact]
    public void Hourly_first_run_computes_promptly_from_now()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Hourly, IsEnabled = true };
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, null, Now);
        Assert.True(next > Now);
        Assert.True(next <= Now.AddHours(2));
    }

    [Fact]
    public void Daily_first_run_with_no_lastRun_is_in_future()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Daily, IsEnabled = true };
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, null, Now);
        Assert.True(next >= Now);
    }

    [Fact]
    public void Daily_from_yesterday_runs_next_2am_when_today_2am_is_in_past()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Daily, IsEnabled = true };
        // Last ran yesterday 02:00; now is today 10:30. Today's 02:00 has
        // already passed, so we must skip to TOMORROW's 02:00 UTC.
        var last = new DateTime(2026, 9, 16, 2, 0, 0, DateTimeKind.Utc);
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, last, Now);
        Assert.Equal(new DateTime(2026, 9, 18, 2, 0, 0, DateTimeKind.Utc), next);
    }

    [Fact]
    public void Daily_snap_to_2am_even_when_last_was_later()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Daily, IsEnabled = true };
        var last = new DateTime(2026, 9, 16, 13, 45, 0, DateTimeKind.Utc);
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, last, Now);
        // 2026-09-16 13:45 → first 02:00 strictly after is 2026-09-17 02:00,
        // which is before now (10:30), so skip to 2026-09-18 02:00 UTC.
        Assert.Equal(new DateTime(2026, 9, 18, 2, 0, 0, DateTimeKind.Utc), next);
    }

    [Fact]
    public void Weekly_snap_to_2am()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Weekly, IsEnabled = true };
        var last = new DateTime(2026, 9, 10, 5, 0, 0, DateTimeKind.Utc);
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, last, Now);
        // Last ran 2026-09-10 05:00; first 02:00 strictly after both anchor and
        // now (2026-09-17 10:30) is 2026-09-18 02:00 UTC.
        Assert.Equal(new DateTime(2026, 9, 18, 2, 0, 0, DateTimeKind.Utc), next);
    }

    [Fact]
    public void Hourly_from_last_run_is_last_plus_one_hour()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Hourly, IsEnabled = true };
        var last = new DateTime(2026, 9, 17, 9, 15, 0, DateTimeKind.Utc);
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, last, Now);
        Assert.Equal(new DateTime(2026, 9, 17, 10, 15, 0, DateTimeKind.Utc), next);
    }

    [Fact]
    public void ComputeNextRunAt_never_returns_past_time()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Hourly, IsEnabled = true };
        // Pretend we last ran in the future (clock skew / paused worker).
        var last = Now.AddHours(2);
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, last, Now);
        Assert.True(next > Now, $"next was {next:O}, must be > now ({Now:O})");
    }

    [Fact]
    public void Manual_compute_returns_sentinel_far_future()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Manual, IsEnabled = true };
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, null, Now);
        Assert.Equal(DateTime.MaxValue, next);
    }

    [Fact]
    public void First_run_for_disabled_schedule_still_returns_a_valid_time()
    {
        var schedule = new BackupScheduleConfig { Frequency = BackupFrequency.Daily, IsEnabled = false };
        // ComputeNextRunAt doesn't gate on IsEnabled — only ShouldRunNow does.
        // This ensures the host can still surface "next run would be at X" even
        // when the schedule is paused.
        var next = BackupScheduleEvaluator.ComputeNextRunAt(schedule, null, Now);
        Assert.NotEqual(DateTime.MinValue, next);
    }
}