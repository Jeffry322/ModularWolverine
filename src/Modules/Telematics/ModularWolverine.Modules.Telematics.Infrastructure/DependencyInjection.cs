using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularWolverine.Modules.Telematics.Application.Common.Contracts;

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
                configureSettings: settings => settings.DisableRetry = true,
                configureDbContextOptions: opts =>
                {
                    opts.UseSnakeCaseNamingConvention();
                    opts.UseNpgsql(npgsql =>
                    {
                        npgsql.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            TelematicsDbContext.Schema);
                    });
                });
            
            builder.Services.AddScoped<ITelematicsDbContext>(sp => sp.GetRequiredService<TelematicsDbContext>());
            return appBuilder;
        }
    }
}