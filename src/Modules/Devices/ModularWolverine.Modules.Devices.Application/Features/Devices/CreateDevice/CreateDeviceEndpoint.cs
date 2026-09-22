using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using ModularWolverine.Modules.Devices.Domain.Devices;
using Wolverine.Attributes;
using Wolverine.Http;

namespace ModularWolverine.Modules.Devices.Application.Features.Devices.CreateDevice;

public static class CreateDeviceEndpoint
{
    public static async Task<ProblemDetails> Validate(
        CreateDeviceCommand command,
        IDevicesDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var deviceExists = await context.Devices
            .AnyAsync(d => EF.Property<string>(d, "_imei") == command.Imei,
                cancellationToken);

        if (deviceExists)
        {
            logger.LogWarning("Device {Imei} already exists", command.Imei);
            
            return new ProblemDetails
            {
                Detail = "Device already exists",
                Status = StatusCodes.Status409Conflict,
            };
        }

        return WolverineContinue.NoProblems;
    }
    
    [Tags(new[] { "Devices", "Create"})]
    [Transactional]
    [WolverinePost(
        "/api/devices/",
        OperationId = "CreateDevice",
        Summary = "Creates a new device",
        Description = "Creates a new device")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(CreateDeviceResponse), StatusCodes.Status201Created)]
    public static CreateDeviceResponse Post(
        CreateDeviceCommand command,
        IDevicesDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Creating device {Imei}", command.Imei);
        
        var device = Device.Create(command.Imei, command.Name);

        context.Devices.Add(device);

        return new CreateDeviceResponse(device.Id);
    }
}