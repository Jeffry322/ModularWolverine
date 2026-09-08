using Microsoft.EntityFrameworkCore;
using ModularWolverine.Modules.Devices.Domain.Devices;

namespace ModularWolverine.Modules.Devices.Application.Common.Contracts;

public interface IDevicesDbContext
{
    DbSet<Device> Devices { get; }
}