using Microsoft.EntityFrameworkCore;
using ModularWolverine.Modules.Telematics.Application.Common.Contracts;

namespace ModularWolverine.Modules.Telematics.Infrastructure;

public sealed class TelematicsDbContext(DbContextOptions<TelematicsDbContext> options)
    : DbContext(options), ITelematicsDbContext
{
    public const string Schema = "telematics";
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelematicsDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}