using Domain.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountDataAccess;

public class AccountProjectionTypeConfig : IEntityTypeConfiguration<AccountProjection>
{
    public void Configure(EntityTypeBuilder<AccountProjection> builder)
    {
        builder.ToTable("AccountProjections");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountHolder)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Balance)
            .HasPrecision(18, 2);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.Version)
            .IsConcurrencyToken();
    }
}
