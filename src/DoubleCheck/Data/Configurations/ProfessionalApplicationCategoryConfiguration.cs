using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class ProfessionalApplicationCategoryConfiguration : IEntityTypeConfiguration<ProfessionalApplicationCategory>
{
    public void Configure(EntityTypeBuilder<ProfessionalApplicationCategory> b)
    {
        b.ToTable("ProfessionalApplicationCategories");
        b.HasKey(x => new { x.ProfessionalApplicationId, x.CategoryId });
    }
}
