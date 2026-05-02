using System.Net;
using System.Text.Json;
using Store.API.Application.Common;
using Store.API.Contracts;

namespace Store.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var traceId = context.TraceIdentifier;

        var (statusCode, code, message, errors) = exception switch
        {
            RequestValidationException vex =>
                (HttpStatusCode.BadRequest, "validation_error", "Validation failed.", vex.Errors),
            InvalidOperationException =>
                (HttpStatusCode.BadRequest, "invalid_operation", exception.Message, (IReadOnlyCollection<string>?)null),
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "unauthorized", "Unauthorized.", (IReadOnlyCollection<string>?)null),
            KeyNotFoundException =>
                (HttpStatusCode.NotFound, "not_found", exception.Message, (IReadOnlyCollection<string>?)null),
            ArgumentException =>
                (HttpStatusCode.BadRequest, "invalid_argument", exception.Message, (IReadOnlyCollection<string>?)null),
            _ =>
                (HttpStatusCode.InternalServerError, "server_error", "An unexpected error occurred.", (IReadOnlyCollection<string>?)null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiErrorResponse.From(code, message, errors, traceId);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
