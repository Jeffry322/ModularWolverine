using ModularWolverine.Modules.Devices.Domain.Devices.Events;
using ModulaWolverine.BuildingBlocks.Domain;

namespace ModularWolverine.Modules.Devices.Domain.Devices;

public class Device : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }

    private string _imei;

    private string? _name;
    
    private DateTime _createdAtUtc;
    
    private DeviceState _state;

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
        _imei = imei;
        _name = name;
        _createdAtUtc = createdAtUtc;
        _state = state;

        Raise(new DeviceCreatedDomainEvent(Id));
    }
}