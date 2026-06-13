using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class VerificationSessionConfiguration : IEntityTypeConfiguration<VerificationSession>
{
    public void Configure(EntityTypeBuilder<VerificationSession> b)
    {
        b.ToTable("VerificationSessions");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.RequesterUserId);
        b.HasIndex(x => new { x.ProfessionalUserId, x.Status });
        b.Property(x => x.AgreedRate).HasPrecision(18, 2);
        b.Property(x => x.QuestionSnapshot).IsRequired();
        b.Property(x => x.AiAnswerSnapshot).IsRequired();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Outcome).HasConversion<int>();
    }
}
