using Microsoft.EntityFrameworkCore;

namespace AccountProjection;

public class AccountProjectionDbContext : DbContext
{
    public AccountProjectionDbContext(DbContextOptions<AccountProjectionDbContext> options) : base(options)
    {
    }

    public DbSet<AccountSummaryProjection> AccountSummaryProjections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AccountProjectionTypeConfig());
    }
}