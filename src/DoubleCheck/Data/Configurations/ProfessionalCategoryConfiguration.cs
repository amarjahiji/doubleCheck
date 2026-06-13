using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class ProfessionalCategoryConfiguration : IEntityTypeConfiguration<ProfessionalCategory>
{
    public void Configure(EntityTypeBuilder<ProfessionalCategory> b)
    {
        b.ToTable("ProfessionalCategories");
        b.HasKey(x => new { x.ProfessionalProfileId, x.CategoryId });
        b.HasIndex(x => x.CategoryId);
    }
}
