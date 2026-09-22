using Wolverine.Http;

namespace ModularWolverine.Modules.Devices.Application.Features.Devices.CreateDevice;

public record CreateDeviceResponse(Guid Id) : CreationResponse($"/api/devices/{Id}");