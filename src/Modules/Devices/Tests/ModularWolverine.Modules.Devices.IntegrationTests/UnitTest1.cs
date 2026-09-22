using System.Net;
using Alba;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularWolverine.Modules.Devices.Application.Features.Devices.CreateDevice;
using ModularWolverine.Modules.Devices.Application.Features.Devices.GetDevice;
using ModularWolverine.Modules.Devices.Domain.Devices;
using ModularWolverine.Modules.Devices.Domain.Devices.Events;
using ModularWolverine.Modules.Devices.Infrastructure.Database;
using ModularWolverine.Modules.Devices.IntegrationTests.Infrastructure;
using Wolverine.EntityFrameworkCore;
using Wolverine.Runtime;
using Wolverine.Tracking;

namespace ModularWolverine.Modules.Devices.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class UnitTest1(IntegrationTestFixture fixture) : IAsyncDisposable
{
    [Fact]
    public async Task GetDeviceEndpoint_ShouldReturn404_WhenIdentifierIsInvalid()
    {
        await fixture.Host.Scenario(_ =>
        {
            _.Get.Url($"/api/devices/definetely-not-imei");
            _.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task GetDeviceEndpoint_ShouldReturn200_WhenOnlyImeiIsPresent()
    {
        string imei = "123";
        var device = Device.Create(imei, "TestDevice");

        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();

        db.Add(device);
        await db.SaveChangesAsync(CancellationToken.None);

        var scenarioResult = await fixture.Host.Scenario(_ =>
        {
            _.Get.Url($"/api/devices/{imei}");
            _.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var response = await scenarioResult.ReadAsJsonAsync<GetDeviceResponse>();
        Assert.Equal(response.Id, device.Id);
    }

    [Fact]
    public async Task CreateDeviceEndpoint_ShouldPublishDomainEvent_WhenRequestIsValid_WithCorrectId()
    {
        var command = new CreateDeviceCommand
        {
            Imei = "123",
            Name = "TestDevice"
        };

        IScenarioResult scenarioResult = null!;

        var tracked = await fixture.Host.ExecuteAndWaitAsync(async () =>
        {
            scenarioResult = await fixture.Host.Scenario(s =>
            {
                s.Post.Json(command).ToUrl("/api/devices");
                s.StatusCodeShouldBe(HttpStatusCode.Created);
            });
        });

        var published = tracked.Sent.SingleMessage<DeviceCreatedDomainEvent>();
        var response = await scenarioResult.ReadAsJsonAsync<CreateDeviceResponse>();
        
        Assert.NotNull(published);
        Assert.Equal(Guid.Empty, response.Id);
        
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        
        var device = db.Devices.FirstOrDefault(x => x.Imei == command.Imei);
        
        Assert.NotNull(device);
        Assert.Equal(command.Name, device.Name);
        Assert.Equal(published.Id, device.Id);
    }

    public async ValueTask DisposeAsync()
    {
        await fixture.Host.ClearAllEnvelopeStorageAsync();

        await fixture.Host.ResetAllDataAsync<DevicesDbContext>();
    }
}