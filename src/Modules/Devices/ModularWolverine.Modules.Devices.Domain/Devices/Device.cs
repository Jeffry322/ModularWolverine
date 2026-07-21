using System.ComponentModel;
using ModularWolverine.Modules.Devices.Domain.Devices.Events;
using ModulaWolverine.BuildingBlocks.Domain;

namespace ModularWolverine.Modules.Devices.Domain.Devices;

public class Device : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }

    private string _imei;

    private DateTime _createdAtUtc;
    
    private DeviceState _state;

    public static Device Create(string imei, DateTime createdAtUtc, DeviceState state)
    {
        return new Device(
            imei,
            createdAtUtc,
            state);
    }

    private Device(
        string imei,
        DateTime createdAtUtc,
        DeviceState state)
    {
        Id = Guid.CreateVersion7();
        _imei = imei;
        _createdAtUtc = createdAtUtc;
        _state = state;
        
        Raise(new DeviceCreatedDomainEvent(Id));
    }
}