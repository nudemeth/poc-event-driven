using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountProjectionDataAccess;

public static class AccountProjectionConfigurator
{
    public static void ConfigureAccountProjectionServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
            configuration?.GetConnectionString("DefaultConnection");

        services.AddDbContext<AccountProjectionDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}