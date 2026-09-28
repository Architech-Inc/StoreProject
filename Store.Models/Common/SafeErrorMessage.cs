using Microsoft.Extensions.Logging;

namespace Store.Models.Common;

/// <summary>
/// Environment-aware formatting of exception messages for user-facing responses.
///
/// <para>
/// <b>Production</b>: returns only the exception type name, e.g.
/// <c>"InvalidOperationException"</c>. The full message is logged server-side
/// via the standard <see cref="ILogger"/> pattern; the caller never sees it,
/// so schema names, SQL fragments, file paths, and any future PII in the
/// exception message never leak.
/// </para>
///
/// <para>
/// <b>Development</b>: returns the type + message verbatim, e.g.
/// <c>"InvalidOperationException: Object reference not set to an instance of an object."</c>.
/// The developer can see exactly what blew up without ssh'ing into the server.
/// Detected via <c>ASPNETCORE_ENVIRONMENT=Development</c>.
/// </para>
///
/// <para>
/// Usage: <c>ErrorMessage = SafeErrorMessage.From(ex, _logger, "OperationName");</c>
/// in any catch block. Pass <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{T}.Instance"/>
/// for code paths that don't yet have a logger injected.
/// </para>
/// </summary>
public static class SafeErrorMessage
{
    private static readonly bool _isDevelopment =
        string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Render a safe error message string from an exception.
    /// Logs the full exception server-side, then returns the env-aware string.
    /// </summary>
    public static string From(Exception ex, ILogger? logger = null, string? operation = null)
    {
        if (logger is not null)
        {
            if (string.IsNullOrWhiteSpace(operation))
            {
                logger.LogError(ex, "Operation failed: {ExceptionType}", ex.GetType().Name);
            }
            else
            {
                logger.LogError(ex, "Operation '{Operation}' failed: {ExceptionType}", operation, ex.GetType().Name);
            }
        }

        var body = _isDevelopment
            ? $"{ex.GetType().Name}: {ex.Message}"
            : ex.GetType().Name;

        return string.IsNullOrWhiteSpace(operation) ? body : $"{operation}: {body}";
    }
}
