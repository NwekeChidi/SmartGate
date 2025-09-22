namespace SmartGate.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => this._domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => this._domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => this._domainEvents.Clear(); 
}
