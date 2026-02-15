using Domain.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMigration.EntityTypeConfigs;

public class AccountEntityTypeConfig : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountHolder)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Balance)
            .HasPrecision(18, 2);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Ignore(a => a.Events);
    }
}
