namespace Store.API.Contracts;

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Code { get; set; } = "error";
    public string Message { get; set; } = "Request failed.";
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
