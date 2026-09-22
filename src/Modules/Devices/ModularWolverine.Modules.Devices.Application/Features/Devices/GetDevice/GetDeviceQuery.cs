namespace ModularWolverine.Modules.Devices.Application.Features.Devices.GetDevice;

public record GetDeviceQuery
{
    public string? Imei { get; set; }
    public string?  Id { get; set; }
}