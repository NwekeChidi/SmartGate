using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartGate.Domain.Common;
using SmartGate.Domain.Visits.Entities;
using SmartGate.Infrastructure.Database.Setup;

namespace SmartGate.Infrastructure.Database;

public class SmartGateDbContext(DbContextOptions<SmartGateDbContext> options) : DbContext(options)
{
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<Driver> Drivers => Set<Driver>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VisitConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new IdempotencyKeyConfiguration());
        modelBuilder.ApplyConfiguration(new DriverConfiguration()); 
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Gather domain events before save
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .ToList();

        var events = aggregates.SelectMany(x => x.Entity.DomainEvents).ToList();

        // Convert to outbox messages (event-ready)
        foreach (var evt in events)
        {
            OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredAtUTC = evt.OccurredAtUTC,
                Type = evt.GetType().FullName ?? evt.GetType().Name,
                Content = JsonSerializer.Serialize(evt, evt.GetType()),
                ProcessedAtUTC = null
            });
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after successful commit
        foreach (var entry in aggregates)
            entry.Entity.ClearDomainEvents();

        return result;
    }
}