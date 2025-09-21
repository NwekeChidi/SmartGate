namespace SmartGate.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
