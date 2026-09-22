using Microsoft.Extensions.Logging;
using ModularWolverine.Modules.Devices.Domain.Devices.Events;

namespace ModularWolverine.Modules.Devices.Application.Features.Devices;

public static class DeviceCreatedDomainEventHandler
{
    public static Task Handle(
        DeviceCreatedDomainEvent @event,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain event handled");
        
        return Task.CompletedTask;
    }
}