using AccountDataAccess;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountEventListener;

public static class EventListenerConfigurator
{
    public static IServiceCollection ConfigureEventListenerServices(this IServiceCollection services, ILambdaContext context)
    {
        // Register the Lambda context as a singleton
        services.AddSingleton(context);

        // Configure PostgreSQL database context
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=accountdb;Username=postgres;Password=postgrespwd";

        services.AddDbContext<AccountDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register the Mediator with vertical slices
        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Scoped;
            opts.Assemblies = [typeof(EventListenerConfigurator).Assembly];
        });

        return services;
    }
}
