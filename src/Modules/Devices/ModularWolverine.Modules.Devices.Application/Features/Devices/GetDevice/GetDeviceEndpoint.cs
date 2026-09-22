using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using Wolverine.Http;

namespace ModularWolverine.Modules.Devices.Application.Features.Devices.GetDevice;

public static class GetDeviceEndpoint
{
    public static Task<GetDeviceResponse?> LoadAsync(
        string identifier,
        IDevicesDbContext context,
        CancellationToken cancellationToken)
    {
        var databaseQuery = context.Devices.AsQueryable();

        if (Guid.TryParse(identifier, out var guid))
        {
            databaseQuery = databaseQuery.Where(d => d.Id == guid);
        }
        else
        {
            databaseQuery = databaseQuery.Where(d => d.Imei == identifier);
        }
        
        return databaseQuery
            .Select(device => new GetDeviceResponse(
                device.Id,
                device.Imei,
                device.Name!))
            .SingleOrDefaultAsync(cancellationToken);
    }
    
    [Tags(new[] { "Devices", "Get"})]
    [WolverineGet(
        "/api/devices/{identifier}",
        OperationId = "GetDeviceByIdentifier",
        Summary = "Retrieves a device by id or IMEI.",
        Description = "Retrieves a device by id or IMEI.")]
    [ProducesResponseType(typeof(GetDeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static GetDeviceResponse Get(
        string identifier,
        [Required] GetDeviceResponse? response)
    {
        return response!;
    }
}