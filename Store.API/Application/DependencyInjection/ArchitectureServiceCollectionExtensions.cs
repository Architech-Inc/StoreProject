using System.Reflection;
using Store.API.Application.Abstractions;
using Store.API.Application.Common;
using Store.API.Application.Auth.Ports;
using Store.API.Application.Users.Ports;
using Store.API.Infrastructure.Auth;
using Store.API.Infrastructure.Users;

namespace Store.API.Application.DependencyInjection;

public static class ArchitectureServiceCollectionExtensions
{
    public static IServiceCollection AddArchitecture(this IServiceCollection services)
    {
        services.AddScoped<IRequestDispatcher, RequestDispatcher>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IUsersPort, UsersPort>();
        services.AddScoped<IAuthPort, AuthPort>();

        RegisterClosedGenericImplementations(services, typeof(IRequestHandler<,>));
        RegisterClosedGenericImplementations(services, typeof(IRequestValidator<>));

        return services;
    }

    private static void RegisterClosedGenericImplementations(IServiceCollection services, Type openGenericType)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var implementationTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToList();

        foreach (var implementation in implementationTypes)
        {
            var serviceInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType)
                .ToList();

            foreach (var serviceInterface in serviceInterfaces)
            {
                services.AddScoped(serviceInterface, implementation);
            }
        }
    }
}
