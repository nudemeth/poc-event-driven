using Microsoft.EntityFrameworkCore;

namespace AccountDataAccess;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<AccountSummaryProjection> AccountSummaryProjections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AccountProjectionTypeConfig());
    }
}