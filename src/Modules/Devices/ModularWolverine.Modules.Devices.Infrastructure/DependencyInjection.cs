using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                });
            return appBuilder;
        }
    }
}