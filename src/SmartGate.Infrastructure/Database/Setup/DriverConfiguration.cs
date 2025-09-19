using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartGate.Domain.Visits.Entities;

namespace SmartGate.Infrastructure.Database.Setup;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> eb)
    {
        eb.ToTable("drivers");
        eb.HasKey(d => d.Id);

        eb.Property(d => d.FirstName)
            .HasMaxLength(128)
            .IsRequired();

        eb.Property(d => d.LastName)
            .HasMaxLength(128)
            .IsRequired();

        eb.HasIndex(d => new { d.LastName, d.FirstName });
    }
}
