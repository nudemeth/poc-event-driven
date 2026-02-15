using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DbMigration;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MigrationDbContext>
{
    public MigrationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MigrationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=accounts;Username=postgres;Password=postgres");

        return new MigrationDbContext(optionsBuilder.Options);
    }
}
