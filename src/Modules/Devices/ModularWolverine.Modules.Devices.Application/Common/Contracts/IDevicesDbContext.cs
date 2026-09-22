using Microsoft.EntityFrameworkCore;
using ModularWolverine.Modules.Devices.Domain.Devices;

namespace ModularWolverine.Modules.Devices.Application.Common.Contracts;

public interface IDevicesDbContext : IAsyncDisposable
{
    DbSet<Device> Devices { get; }
    
    Task <int> SaveChangesAsync(CancellationToken cancellationToken = default);
}