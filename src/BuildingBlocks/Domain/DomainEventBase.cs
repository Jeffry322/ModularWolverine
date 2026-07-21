namespace ModulaWolverine.BuildingBlocks.Domain;

public abstract class DomainEventBase : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccuredAtUtc { get; }
    
    protected DomainEventBase()
    {
        Id = Guid.CreateVersion7();
        OccuredAtUtc = DateTime.UtcNow;
    }
}