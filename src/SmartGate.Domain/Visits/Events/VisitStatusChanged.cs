using SmartGate.Domain.Common;

namespace SmartGate.Domain.Visits.Events;

public sealed class VisitStatusChanged(Guid visitId, VisitStatus oldStatus, VisitStatus newStatus, DateTime occurredAtUtc) : IDomainEvent
{
    public Guid VisitId { get; } = visitId;
    public VisitStatus OldStatus { get; } = oldStatus;
    public VisitStatus NewStatus { get; } = newStatus;
    public DateTime OccurredAtUtc { get; } = occurredAtUtc;
}