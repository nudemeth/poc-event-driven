using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccountProjectionDataAccess;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccountProjectionDbContext>
{
    public AccountProjectionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccountProjectionDbContext>();
        optionsBuilder.UseNpgsql();

        return new AccountProjectionDbContext(optionsBuilder.Options);
    }
}
