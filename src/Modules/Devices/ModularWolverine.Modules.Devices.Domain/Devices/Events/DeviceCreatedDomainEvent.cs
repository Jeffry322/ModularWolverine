using ModulaWolverine.BuildingBlocks.Domain;

namespace ModularWolverine.Modules.Devices.Domain.Devices.Events;

public class DeviceCreatedDomainEvent(Guid DeviceId) : DomainEventBase;