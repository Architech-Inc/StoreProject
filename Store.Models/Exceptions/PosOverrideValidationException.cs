using Store.Models.Billing;

namespace Store.Models.Exceptions;

/// <summary>
/// Wave 23.C — thrown when POS checkout validation fails for one or more
/// per-line discount override requests. The InvoicesController catches
/// this and translates it into HTTP 422 Unprocessable Entity with the
/// failure detail as the response body.
/// </summary>
public class PosOverrideValidationException : Exception
{
    public PosOverrideValidationException(PosOverrideValidationFailure failure, string message)
        : base(message)
    {
        Failure = failure;
    }

    public PosOverrideValidationFailure Failure { get; }
}
