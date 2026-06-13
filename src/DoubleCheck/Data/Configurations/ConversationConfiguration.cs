using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("Conversations");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.HasMany(x => x.Messages).WithOne(m => m.Conversation!)
            .HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
    }
}
