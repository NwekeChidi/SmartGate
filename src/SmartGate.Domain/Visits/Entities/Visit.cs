using SmartGate.Domain.Common;
using SmartGate.Domain.Visits.Events;

namespace SmartGate.Domain.Visits.Entities;

public sealed class Visit : AggregateRoot
{
    private readonly List<Activity> _activities = [];

    public Guid Id { get; }
    public VisitStatus Status { get; private set; } = VisitStatus.PreRegistered;
    public Truck Truck { get; }
    public Driver Driver { get; }
    public IReadOnlyList<Activity> Activities => _activities;
    public DateTime CreatedAtUTC { get; }
    public DateTime UpdatedAtUTC { get; private set; }
    public string CreatedBy { get; }
    public string UpdatedBy { get; private set; }

    private static readonly IReadOnlyDictionary<VisitStatus, VisitStatus?> _linearNext = new Dictionary<VisitStatus, VisitStatus?>()
    {
        { VisitStatus.PreRegistered, VisitStatus.AtGate },
        { VisitStatus.AtGate, VisitStatus.OnSite },
        { VisitStatus.OnSite, VisitStatus.Completed },
        { VisitStatus.Completed, null }
    };

    private Visit()
    {
        // Required for EF Core materialization; properties are non-nullable but will be set by EF.
        Truck = null!;
        Driver = null!;
        CreatedBy = null!;
        UpdatedBy = null!;
    } // EF Core

    public Visit(Truck truck, Driver driver, IEnumerable<Activity> activities, Guid? id = null, DateTime? nowUTC = null, string? createdBy = null)
    {
        if (truck is null) throw new NullReferenceInAggregateException(nameof(truck));
        if (driver is null) throw new NullReferenceInAggregateException(nameof(driver));
        if (activities is null) throw new NullReferenceInAggregateException(nameof(activities));

        var activitiesList = activities.ToList();
        if (activitiesList.Count == 0) throw new ActivitiesRequiredException();

        Id = id ?? Guid.NewGuid();
        Truck = truck;
        Driver = driver;

        _activities.AddRange(activitiesList);

        var ts = nowUTC ?? DateTime.UtcNow;
        CreatedAtUTC = ts;
        UpdatedAtUTC = ts;
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "SYSTEM" : createdBy!;
        UpdatedBy = CreatedBy;
    }

    public void UpdateStatus(VisitStatus next, string updatedBy, DateTime? nowUTC = null)
    {
        if (Status == next) return; // idempotent no-op

        if (Status == VisitStatus.Completed) throw new CompletedIsTerminalException();

        if (!_linearNext.TryGetValue(Status, out var expected) || expected is null || expected != next)
            throw new InvalidStatusTransitionException(Status, next);

        var oldStatus = Status;
        Status = next;
        UpdatedAtUTC = nowUTC ?? DateTime.UtcNow;
        UpdatedBy = updatedBy;

        RaiseDomainEvent(new VisitStatusChanged(Id, oldStatus, next, UpdatedAtUTC));
    }
}
