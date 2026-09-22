using Microsoft.EntityFrameworkCore;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using ModularWolverine.Modules.Devices.Domain.Devices;

namespace ModularWolverine.Modules.Devices.Infrastructure.Database;

public sealed class DevicesDbContext(DbContextOptions<DevicesDbContext> options)
    : DbContext(options), IDevicesDbContext
{
    public const string Schema = "devices";
    
    public DbSet<Device> Devices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevicesDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}