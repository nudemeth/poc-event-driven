using Microsoft.Extensions.DependencyInjection;
using Domain.Account;
using AccountProjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application;

public static class ApplicationConfigurator
{
    public static void ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TransferService>();

        services.AddMediator(opts =>
        {
            opts.ServiceLifetime = ServiceLifetime.Scoped;
            opts.Assemblies = [typeof(ApplicationConfigurator)];
        });

        services.ConfigureAccountProjectionServices(configuration);
    }
}