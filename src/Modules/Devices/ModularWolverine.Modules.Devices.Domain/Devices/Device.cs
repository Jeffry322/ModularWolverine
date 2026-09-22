using ModularWolverine.Modules.Devices.Domain.Devices.Events;
using ModulaWolverine.BuildingBlocks.Domain;

namespace ModularWolverine.Modules.Devices.Domain.Devices;

public class Device : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }

    public string Imei { get; private set; }

    public string? Name { get; private set; }
    
    public DateTime CreatedAtUtc { get; private set; }
    
    public DeviceState State { get;  private set; }

    public static Device Create(
        string imei,
        string? name)
    {
        return new Device(
            imei,
            name,
            DateTime.UtcNow,
            DeviceState.Ok);
    }

    private Device(
        string imei,
        string? name,
        DateTime createdAtUtc,
        DeviceState state)
    {
        Id = Guid.CreateVersion7();
        Imei = imei;
        Name = name;
        CreatedAtUtc = createdAtUtc;
        State = state;

        Raise(new DeviceCreatedDomainEvent(Id));
    }
}