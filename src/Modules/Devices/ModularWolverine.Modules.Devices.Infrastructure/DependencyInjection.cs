using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;

namespace ModularWolverine.Modules.Devices.Infrastructure;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddDevicesModule(IHostApplicationBuilder appBuilder)
        {
            return builder
                .AddPersistence(appBuilder);
        }

        private IHostApplicationBuilder AddPersistence(IHostApplicationBuilder appBuilder)
        {
            appBuilder.AddNpgsqlDbContext<DevicesDbContext>(
                connectionName: "modular-wolverine",
                configureDbContextOptions: opts =>
                {
                    opts.UseSnakeCaseNamingConvention();
                    opts.UseNpgsql(npgsql =>
                    {
                        npgsql.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            DevicesDbContext.Schema);
                    });
                });
            
            builder.Services.AddScoped<IDevicesDbContext>(sp => sp.GetRequiredService<DevicesDbContext>());
            return appBuilder;
        }
    }
}