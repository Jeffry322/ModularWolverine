namespace ModularWolverine.Modules.Devices.Application.Features.Devices.CreateDevice;

public sealed record CreateDeviceCommand
{
    public required string Imei { get; init; }
    public required string Name { get; init; }
}