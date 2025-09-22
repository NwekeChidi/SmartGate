using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Infrastructure.Database.Setup;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> eb)
    {
        eb.ToTable("activities");
        eb.HasKey(a => a.Id);

        eb.Property(a => a.Type).HasConversion<int>().IsRequired();
        eb.Property(a => a.UnitNumberRaw).HasMaxLength(32).IsRequired();
        eb.Property(a => a.UnitNumberNormalized).HasMaxLength(32).IsRequired();

        eb.Property<Guid>("VisitId").IsRequired();
        eb.HasIndex("VisitId");
        eb.HasIndex(a => a.UnitNumberNormalized);
    }
}
