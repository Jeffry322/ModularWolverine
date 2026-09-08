namespace ModularWolverine.Modules.Devices.Application.Features.Devices;

public sealed record CreateDeviceCommand
{
    public string Imei { get; init; }
    public string Name { get; init; }
}