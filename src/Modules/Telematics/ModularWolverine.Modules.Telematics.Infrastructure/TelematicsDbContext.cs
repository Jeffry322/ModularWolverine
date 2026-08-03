using Microsoft.EntityFrameworkCore;

namespace ModularWolverine.Modules.Telematics.Infrastructure;

public sealed class TelematicsDbContext(DbContextOptions<TelematicsDbContext> options)
    : DbContext(options)
{
    public const string Schema = "telematics";
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelematicsDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}