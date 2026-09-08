using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using ModularWolverine.Modules.Devices.Domain.Devices;
using Wolverine.Attributes;
using Wolverine.Http;

namespace ModularWolverine.Modules.Devices.Application.Features.Devices;

public static class CreateDeviceEndpoint
{
    [Tags(new[] { "Devices" })]
    [Transactional]
    [WolverinePost(
        "/api/devices",
        OperationId = "CreateDevice",
        Summary = "Creates a new device",
        Description = "Creates a new device")]
    public static Task Post(
        CreateDeviceCommand command,
        IDevicesDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Creating device {Imei}", command.Imei);
        
        var device = Device.Create(command.Imei, command.Name);

        context.Devices.Add(device);
        
        return Task.CompletedTask;
    }
}