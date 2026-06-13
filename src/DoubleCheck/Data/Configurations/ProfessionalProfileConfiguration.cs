using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class ProfessionalProfileConfiguration : IEntityTypeConfiguration<ProfessionalProfile>
{
    public void Configure(EntityTypeBuilder<ProfessionalProfile> b)
    {
        b.ToTable("ProfessionalProfiles");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId).IsUnique();
        b.Property(x => x.RatePerAnswer).HasPrecision(18, 2);
        b.Property(x => x.Bio).HasMaxLength(2000);
        b.HasMany(x => x.Categories).WithOne(c => c.ProfessionalProfile!)
            .HasForeignKey(c => c.ProfessionalProfileId).OnDelete(DeleteBehavior.Cascade);
    }
}
