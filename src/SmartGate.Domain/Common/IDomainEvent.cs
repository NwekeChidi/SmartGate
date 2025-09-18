namespace SmartGate.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAtUTC { get; }
}
