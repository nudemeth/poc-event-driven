using Microsoft.Extensions.DependencyInjection;
using Domain.Account;

namespace Application;

public static class ApplicationConfigurator
{
    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<TransferService>();

        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Scoped;
            opts.Assemblies = [typeof(ApplicationConfigurator)];
        });
    }
}