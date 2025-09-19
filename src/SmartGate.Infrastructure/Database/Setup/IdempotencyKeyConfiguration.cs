using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartGate.Infrastructure.Database.Setup;

public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> eb)
    {
        eb.ToTable("idempotency_keys");
        eb.HasKey(x => x.Key);
        eb.Property(x => x.Key).HasMaxLength(128);
        eb.Property(x => x.CreatedAtUTC).IsRequired();
        eb.HasIndex(x => x.CreatedAtUTC);
    }
}
