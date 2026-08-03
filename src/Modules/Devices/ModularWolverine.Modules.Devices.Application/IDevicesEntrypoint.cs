namespace ModularWolverine.Modules.Devices.Application;

public interface IDevicesEntrypoint
{
    Task InvokeAsync(object message);
    
    Task<TResponse> InvokeWithResultAsync<TResponse>(object message);
}