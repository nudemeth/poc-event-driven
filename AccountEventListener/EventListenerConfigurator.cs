using AccountProjection;
using Amazon.Lambda.Core;
using Domain.Account;
using Microsoft.EntityFrameworkCore;
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
            opts.ServiceLifetime = ServiceLifetime.Scoped;
            opts.Assemblies = [typeof(EventListenerConfigurator).Assembly, typeof(AccountEntity).Assembly];
        });

        services.ConfigureAccountProjectionServices();

        return services;
    }
}
