namespace Store.Models.DTOs.Common;

/// <summary>
/// Canonical error response shape returned by every API endpoint.
/// Mirror of <c>Store.API.Contracts.ApiErrorResponse</c> so library code
/// (ControlPlane + Tenant Portal) can build error envelopes without
/// depending on the Store.API assembly.
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Code { get; set; } = "error";

    /// <summary>
    /// Default message is intentionally non-empty and actionable: when a
    /// caller forgets to set a specific message we still tell the operator
    /// where to look instead of saying nothing. Override per-call when a
    /// more specific message is available.
    /// </summary>
    public string Message { get; set; } = "The request could not be completed. See server logs for details.";

    public IReadOnlyCollection<string>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiErrorResponse From(
        string code,
        string message,
        IReadOnlyCollection<string>? errors = null,
        string? traceId = null)
    {
        return new ApiErrorResponse
        {
            Code = code,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }
}