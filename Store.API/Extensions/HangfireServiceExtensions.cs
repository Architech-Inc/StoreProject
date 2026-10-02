using Hangfire;
using Hangfire.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Models.Interfaces.Services;
using System.Transactions;
using Store.API.Jobs;

namespace Store.API.Extensions;

public static class HangfireServiceExtensions
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("Default");
        var connectionString = NormalizeConnectionString(rawConnectionString);

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
            {
                TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "Hangfire"
            })));

        services.AddHangfireServer();

        return services;
    }

    private static string NormalizeConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return string.Empty;

        // Strip trailing semicolons, empty tokens, and empty-value keys (such as 'Password=')
        // to prevent Hangfire.MySql internal ToString() from throwing IndexOutOfRangeException.
        var parts = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p =>
            {
                var eqIdx = p.IndexOf('=');
                if (eqIdx <= 0) return false;
                var val = p.Substring(eqIdx + 1).Trim();
                return !string.IsNullOrEmpty(val);
            });

        return string.Join(";", parts);
    }

    public static void ScheduleProcurementJobs(this IServiceProvider serviceProvider)
    {
        // Run daily at midnight
        RecurringJob.AddOrUpdate<IProcurementAutomationService>(
            "automated-procurement-scan",
            service => service.EvaluateInventoryThresholdsAsync(CancellationToken.None),
            Cron.Daily);
    }

    public static void SchedulePayrollJobs(this IServiceProvider serviceProvider)
    {
        // Run on the 28th of every month at midnight
        RecurringJob.AddOrUpdate<PayrollJobs>(
            "automated-monthly-payroll-draft",
            job => job.DraftMonthlyPayrollAsync(),
            "0 0 28 * *");
    }
}
