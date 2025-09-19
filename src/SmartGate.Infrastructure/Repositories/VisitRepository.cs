using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Infrastructure.Persistence.Setup;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> eb)
    {
        eb.ToTable("visits");
        eb.HasKey(v => v.Id);

        eb.Property(v => v.Status).HasConversion<int>().IsRequired();
        eb.Property(v => v.CreatedAtUTC).IsRequired();
        eb.Property(v => v.UpdatedAtUTC).IsRequired();
        eb.Property(v => v.CreatedBy).HasMaxLength(128).IsRequired();
        eb.Property(v => v.UpdatedBy).HasMaxLength(128).IsRequired();

        eb.OwnsOne(v => v.Truck, tb =>
        {
            tb.Property(p => p.LicensePlateRaw)
              .HasColumnName("truck_plate_raw")
              .HasMaxLength(32)
              .IsRequired();

            tb.Property(p => p.LicensePlateNormalized)
              .HasColumnName("truck_plate_normalized")
              .HasMaxLength(32)
              .IsRequired();

            tb.WithOwner();
        });

        eb.Property<string>("DriverId").IsRequired();
        eb.HasOne(v => v.Driver)
            .WithMany()
            .HasForeignKey("DriverId")
            .OnDelete(DeleteBehavior.Restrict);


        eb.HasMany(v => v.Activities)
            .WithOne()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Cascade);

        eb.HasIndex(v => v.Status);
        eb.HasIndex(v => v.CreatedAtUTC);
        eb.HasIndex("DriverId");
    }
}
