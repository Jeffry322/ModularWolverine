using ModularWolverine.Modules.Telematics.Application;
using Wolverine;

namespace ModularWolverine.Modules.Telematics.Infrastructure;

public sealed class TelematicsEntrypoint(IMessageBus bus) : ITelematicsEntrypoint
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