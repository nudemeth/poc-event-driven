using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountDataAccess;

public static class AccountDataAccessConfigurator
{
    public static void ConfigureAccountDataAccessServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
            configuration?.GetConnectionString("DefaultConnection");

        services.AddDbContext<AccountDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}