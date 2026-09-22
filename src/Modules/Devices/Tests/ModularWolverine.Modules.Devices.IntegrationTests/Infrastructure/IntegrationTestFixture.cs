using Alba;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace ModularWolverine.Modules.Devices.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.3")
        .WithName("modular-wolverine-testcontainers-postgres")
        .WithDatabase("modular_wolverine_tests")
        .WithUsername("postgres")
        .WithPassword("modular-wolverine-tests")
        .WithVolumeMount("modular-wolverine-testcontainers-postgres-data", "/var/lib/postgresql")
        .WithLabel("reuse-id", "modular-wolverine-integration-tests")
        .WithReuse(true)
        .Build();
    private IAlbaHost? _host;

    public IAlbaHost Host => _host ?? throw new InvalidOperationException("The fixture has not started.");

    public async ValueTask InitializeAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        try
        {
            // The PostgreSQL module waits for the database to be ready.
            await _postgres.StartAsync(timeout.Token);

            _host = await AlbaHost.For<global::Program>(host => host
                .UseEnvironment(Environments.Development)
                .UseSetting("ConnectionStrings:modular-wolverine", _postgres.GetConnectionString()));
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
            _host = null;
        }

        // Disposing a reusable Testcontainer stops it. Leave PostgreSQL running
        // intentionally so subsequent test runs reuse the container and its data.
    }
}
