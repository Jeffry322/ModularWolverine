using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ModularWolverine.Modules.Telematics.Infrastructure;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddTelematicsModule(IHostApplicationBuilder appBuilder)
        {
            return builder
                .AddPersistence(appBuilder);
        }

        private IHostApplicationBuilder AddPersistence(IHostApplicationBuilder appBuilder)
        {
            appBuilder.AddNpgsqlDbContext<TelematicsDbContext>(
                connectionName: "modular-wolverine",
                configureDbContextOptions: opts =>
                {
                    opts.UseSnakeCaseNamingConvention();
                });
            return appBuilder;
        }
    }
}