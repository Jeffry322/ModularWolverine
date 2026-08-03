using ModularWolverine.Modules.Devices.Application;
using Wolverine;

namespace ModularWolverine.Modules.Devices.Infrastructure;

public sealed class DevicesEntrypoint(IMessageBus bus) : IDevicesEntrypoint
{
    public async Task InvokeAsync(object message)
    {
        await bus.InvokeAsync(message);
    }

    public async Task<TResponse> InvokeWithResultAsync<TResponse>(object message)
    {
        return await bus.InvokeAsync<TResponse>(message);
    }
}