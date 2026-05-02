using Store.API.Application.Abstractions;

namespace Store.API.Application.Common;

public class RequestDispatcher : IRequestDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public RequestDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = _serviceProvider.GetServices(behaviorType).Cast<dynamic>().ToList();

        Func<Task<TResponse>> pipeline = () => (Task<TResponse>)handler.HandleAsync((dynamic)request, ct);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var next = pipeline;
            pipeline = () => (Task<TResponse>)behavior.HandleAsync((dynamic)request, ct, next);
        }

        return await pipeline();
    }
}
