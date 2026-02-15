using Domain.Account;
using DbMigration.EntityTypeConfigs;
using Microsoft.EntityFrameworkCore;

namespace DbMigration;

public class MigrationDbContext : DbContext
{
    public MigrationDbContext(DbContextOptions<MigrationDbContext> options) : base(options)
    {
    }

    public DbSet<AccountEntity> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AccountEntityTypeConfig());
    }
}