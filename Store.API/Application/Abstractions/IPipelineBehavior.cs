namespace Store.API.Application.Abstractions;

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<Task<TResponse>> next
    );
}
