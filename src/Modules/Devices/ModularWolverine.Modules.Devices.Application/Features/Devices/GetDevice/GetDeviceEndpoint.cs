using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using Wolverine.Http;

namespace ModularWolverine.Modules.Devices.Application.Features.Devices.GetDevice;

public static class GetDeviceEndpoint
{
    public static string[] Validate(GetDeviceQuery query)
    {
        var hasId = !string.IsNullOrWhiteSpace(query.Id);
        var hasImei = !string.IsNullOrWhiteSpace(query.Imei);

        return (hasId, hasImei) switch
        {
            (false, false) => ["Either 'id' or 'imei' must be specified."],
            (true, true)   => ["Specify either 'id' or 'imei', not both."],
            _ => []
        };
    }
    
    public static Task<GetDeviceResponse?> LoadAsync(
        GetDeviceQuery query,
        IDevicesDbContext context,
        CancellationToken cancellationToken)
    {
        var databaseQuery = context.Devices.AsQueryable();

        if (query.Imei is not null)
        {
            databaseQuery = databaseQuery.Where(d => query.Imei == EF.Property<string>(d,"_imei"));
        }
        if (query.Id is not null)
        {
            databaseQuery = databaseQuery.Where(d => query.Id == EF.Property<string>(d,"_id"));
        }
        
        return databaseQuery
            .Select(device => new GetDeviceResponse(
                device.Id,
                EF.Property<string>(device, "_imei"),
                EF.Property<string?>(device, "_name")!))
            .SingleOrDefaultAsync(cancellationToken);
    }
    
    [Tags(new[] { "Devices", "Get"})]
    [WolverineGet(
        "/api/devices",
        OperationId = "GetDeviceByIdentifier",
        Summary = "Retrieves a device by id or IMEI.",
        Description = "Retrieves a device by id or IMEI.")]
    [ProducesResponseType(typeof(GetDeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static GetDeviceResponse Get(
        [FromQuery] GetDeviceQuery query,
        [Required] GetDeviceResponse? response)
    {
        return response!;
    }
}