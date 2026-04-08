using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountProjection;

public class AccountProjectionTypeConfig : IEntityTypeConfiguration<AccountSummaryProjection>
{
    public void Configure(EntityTypeBuilder<AccountSummaryProjection> builder)
    {
        builder.ToTable("AccountSummaryProjections");

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
