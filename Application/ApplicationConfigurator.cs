using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationConfigurator
{
    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Scoped;
            opts.Assemblies = [typeof(ApplicationConfigurator)];
        });
    }
}