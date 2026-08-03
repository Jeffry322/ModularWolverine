using Microsoft.EntityFrameworkCore;

namespace ModularWolverine.Modules.Devices.Infrastructure;

public sealed class DevicesDbContext(DbContextOptions<DevicesDbContext> options)
    : DbContext(options)
{
    public const string Schema = "devices";
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevicesDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}