namespace Store.API.Application.Abstractions;

public interface IRequestValidator<in TRequest>
{
    Task<IReadOnlyCollection<string>> ValidateAsync(TRequest request, CancellationToken ct = default);
}
