using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ModularWolverine.Modules.Telematics.Infrastructure.Database;

public sealed class TelematicsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TelematicsDbContext>
{
    public TelematicsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TelematicsDbContext>();

        options.UseNpgsql(npgsql =>
                npgsql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    TelematicsDbContext.Schema))
            .UseSnakeCaseNamingConvention();

        return new TelematicsDbContext(options.Options);
    }
}
