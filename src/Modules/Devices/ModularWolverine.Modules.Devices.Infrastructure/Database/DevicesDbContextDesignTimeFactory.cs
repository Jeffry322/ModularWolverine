using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ModularWolverine.Modules.Devices.Infrastructure.Database;

public sealed class DevicesDbContextDesignTimeFactory : IDesignTimeDbContextFactory<DevicesDbContext>
{
    public DevicesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>();

        options.UseNpgsql(npgsql =>
                npgsql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    DevicesDbContext.Schema))
            .UseSnakeCaseNamingConvention();
        
        return new DevicesDbContext(options.Options);
    }
}