using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartGate.Infrastructure.Database.Setup;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> eb)
    {
        eb.ToTable("outbox_messages");
        eb.HasKey(x => x.Id);
        eb.Property(x => x.Type).HasMaxLength(256).IsRequired();
        eb.Property(x => x.Content).IsRequired();
        eb.Property(x => x.OccurredAtUTC).IsRequired();
        eb.HasIndex(x => x.ProcessedAtUTC);
    }
}
