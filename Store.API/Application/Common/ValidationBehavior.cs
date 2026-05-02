using Store.API.Application.Abstractions;

namespace Store.API.Application.Common;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IRequestValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IRequestValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken ct, Func<Task<TResponse>> next)
    {
        if (_validators.Any())
        {
            var failures = new List<string>();
            foreach (var validator in _validators)
            {
                var errors = await validator.ValidateAsync(request, ct);
                failures.AddRange(errors);
            }

            if (failures.Count > 0)
            {
                throw new RequestValidationException(failures);
            }
        }

        return await next();
    }
}
