namespace Store.API.Application.Abstractions;

public interface IRequestDispatcher
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
