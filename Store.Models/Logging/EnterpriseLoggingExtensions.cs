using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Store.Models.Logging;

/// <summary>
/// Enterprise structured logging extensions providing centralized log shipping to Seq (OPS-07)
/// alongside local console output and rich context enrichment.
/// </summary>
public static class EnterpriseLoggingExtensions
{
    /// <summary>
    /// Configures Serilog structured enterprise logging with Console and Seq log shipping sinks (OPS-07).
    /// If Seq:ServerUrl is configured or SEQ_SERVER_URL environment variable is set, logs are shipped to Seq.
    /// In the absence of a Seq server URL (such as local dev or automated tests), falls back safely to Console logging.
    /// </summary>
    public static WebApplicationBuilder ConfigureEnterpriseLogging(
        this WebApplicationBuilder builder,
        string applicationName)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            ConfigureLogger(configuration, context.Configuration, context.HostingEnvironment.EnvironmentName, applicationName);
        });

        return builder;
    }

    /// <summary>
    /// Configures a <see cref="LoggerConfiguration"/> with enterprise defaults and Seq sink if configured.
    /// </summary>
    public static LoggerConfiguration ConfigureLogger(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration,
        string environmentName,
        string applicationName)
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogEventLevel.Warning)
            .ReadFrom.Configuration(appConfiguration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environmentName);

        // Standard console output
        configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj}{NewLine}{Exception}");

        // OPS-07: Centralized Seq log shipping
        var seqUrl = appConfiguration["Seq:ServerUrl"]
            ?? Environment.GetEnvironmentVariable("SEQ_SERVER_URL");

        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            var apiKey = appConfiguration["Seq:ApiKey"]
                ?? Environment.GetEnvironmentVariable("SEQ_API_KEY");

            configuration.WriteTo.Seq(
                serverUrl: seqUrl,
                apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey);
        }

        return configuration;
    }
}
