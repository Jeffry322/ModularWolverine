using JasperFx.CodeGeneration.Model;
using JasperFx.CodeGeneration.Services;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;

namespace ModularWolverine.ApiService;

public sealed class DevicesDbContextVariableSource : IVariableSource
{
    public bool Matches(Type type)
    {
        return type == typeof(IDevicesDbContext);  
    }
    
    public Variable Create(Type type)
    {
        var frame = new LazyServiceLocationFrame(type);

        frame.Variable.OverrideName("devicesContextAbstraction");

        return frame.Variable;
    }
}