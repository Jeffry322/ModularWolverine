using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularWolverine.Modules.Devices.Application.Features.Devices.GetDevice;
using ModularWolverine.Modules.Devices.Domain.Devices;
using ModularWolverine.Modules.Devices.Infrastructure.Database;
using ModularWolverine.Modules.Devices.IntegrationTests.Infrastructure;

namespace ModularWolverine.Modules.Devices.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class UnitTest1(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetDeviceEndpoint_ShouldReturn400_WhenQueryIsInvalid()
    {
        await fixture.Host.Scenario(_ =>
        {
            _.Get.Url($"/api/devices");
            _.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task GetDeviceEndpoint_ShouldReturn200_WhenOnlyImeiIsPresent()
    {
        string imei = "123";
        var device = Device.Create(imei, "TestDevice");

        try
        {
            await using var scope = fixture.Host.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();

            db.Add(device);
            await db.SaveChangesAsync(CancellationToken.None);

            var scenarioResult = await fixture.Host.Scenario(_ =>
            {
                _.Get.Url($"/api/devices?imei={imei}");
                _.StatusCodeShouldBe(HttpStatusCode.OK);
            });

            var response = await scenarioResult.ReadAsJsonAsync<GetDeviceResponse>();
            Assert.Equal(response.Id, device.Id);
        }   
        finally
        {
            await using var scope = fixture.Host.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
            
            await db.Devices
                .Where(d => d.Id == device.Id)
                .ExecuteDeleteAsync(CancellationToken.None);
        }
    }
}
