using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class ProfessionalApplicationConfiguration : IEntityTypeConfiguration<ProfessionalApplication>
{
    public void Configure(EntityTypeBuilder<ProfessionalApplication> b)
    {
        b.ToTable("ProfessionalApplications");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.UserId, x.Status });
        b.Property(x => x.RequestedRate).HasPrecision(18, 2);
        b.Property(x => x.Bio).HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<int>();
        b.HasMany(x => x.Categories).WithOne(c => c.ProfessionalApplication!)
            .HasForeignKey(c => c.ProfessionalApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}
