namespace ModularWolverine.Modules.Telematics.Application;

public interface ITelematicsEntrypoint
{
    Task InvokeAsync(object message);
    
    Task<TResponse> InvokeWithResultAsync<TResponse>(object message);
}