using ModulaWolverine.BuildingBlocks.Domain;

namespace ModularWolverine.Modules.Telematics.Domain.Devices;

public class Device : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }

    private string _name;

    private string _externalId;

    private DateTime _createdAtUtc;
}