using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> b)
    {
        b.ToTable("WalletTransactions");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Reason).HasMaxLength(500);
    }
}
