namespace Store.API.Application.Common;

public class RequestValidationException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public RequestValidationException(IReadOnlyCollection<string> errors)
        : base("Validation failed for the request.")
    {
        Errors = errors;
    }
}
