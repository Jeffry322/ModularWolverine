namespace ModulaWolverine.BuildingBlocks.Domain;

public abstract class Entity
{
    private List<IDomainEvent> _domainEvents;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent @event)
    {
        _domainEvents ??= [];
        _domainEvents.Add(@event);   
    }
}