using System.Net;
using System.Text.Json;
using Store.API.Application.Common;
using Store.Models.Common;
using Store.Models.DTOs.Common;

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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var traceId = context.TraceIdentifier;

        if (exception is RequestValidationException rvEx)
        {
            _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", rvEx.Errors));
        }

        var (statusCode, code, message, errors) = exception switch
        {
            RequestValidationException vex =>
                (HttpStatusCode.BadRequest, ErrorCode.ValidationError, "Validation failed.", vex.Errors),
            InvalidOperationException =>
                (HttpStatusCode.BadRequest, ErrorCode.InvalidRequest, SafeErrorMessage.From(exception, _logger, "InvalidOperation"), (IReadOnlyCollection<string>?)null),
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, ErrorCode.Unauthorized, "Unauthorized.", (IReadOnlyCollection<string>?)null),
            KeyNotFoundException =>
                (HttpStatusCode.NotFound, ErrorCode.NotFound, SafeErrorMessage.From(exception, _logger, "NotFound"), (IReadOnlyCollection<string>?)null),
            ArgumentException =>
                (HttpStatusCode.BadRequest, ErrorCode.InvalidRequest, SafeErrorMessage.From(exception, _logger, "Argument"), (IReadOnlyCollection<string>?)null),
            _ =>
                (HttpStatusCode.InternalServerError, ErrorCode.Internal, "An unexpected error occurred.", (IReadOnlyCollection<string>?)null)
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
