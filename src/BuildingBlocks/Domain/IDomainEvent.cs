namespace ModulaWolverine.BuildingBlocks.Domain;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccuredAtUtc { get; }
}