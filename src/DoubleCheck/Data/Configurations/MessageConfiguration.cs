using DoubleCheck.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoubleCheck.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("Messages");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ConversationId);
        b.Property(x => x.Sender).HasConversion<int>();
        b.Property(x => x.Content).IsRequired();
    }
}
