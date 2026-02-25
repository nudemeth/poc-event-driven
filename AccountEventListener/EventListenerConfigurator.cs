using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AccountEventListener;

public static class EventListenerConfigurator
{
    public static IServiceCollection ConfigureEventListenerServices(this IServiceCollection services, ILambdaContext context)
    {
        // Register the Lambda context as a singleton
        services.AddSingleton(context);

        // Register the Mediator with vertical slices
        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Transient;
            opts.Assemblies = [typeof(EventListenerConfigurator)];
        });

        return services;
    }
}
