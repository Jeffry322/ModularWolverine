using ModularWolverine.Modules.Devices.Infrastructure;
using ModularWolverine.Modules.Telematics.Infrastructure;
using ModularWolverine.Modules.Devices.Application;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using ModularWolverine.Modules.Telematics.Application;
using ModulaWolverine.BuildingBlocks.Domain;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(options =>
{
    options.UseRuntimeCompilation();
    options.Discovery.IncludeAssembly(typeof(ModularWolverine.Modules.Devices.Application.AssemblyReference).Assembly);
    options.Discovery.IncludeAssembly(typeof(ModularWolverine.Modules.Telematics.Application.AssemblyReference).Assembly);
    
    var connectionString = builder.Configuration.GetConnectionString("modular-wolverine")!;

    options.PersistMessagesWithPostgresql(connectionString, "wolverine");

    options.Policies.UseDurableLocalQueues();
    
    options.Services.AddScoped<IDevicesDbContext>(sp =>
        sp.GetRequiredService<DevicesDbContext>());
    
    options.UseEntityFrameworkCoreTransactions()
        .WithDbContextAbstraction<IDevicesDbContext, DevicesDbContext>();

    options.PublishDomainEventsFromEntityFrameworkCore<Entity>(entity => entity.DomainEvents);
});

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddWolverineHttp();

builder.Services.AddOpenApi();

builder.AddDevicesModule(builder);

builder.AddTelematicsModule(builder);

builder.Services.AddScoped<ITelematicsEntrypoint, TelematicsEntrypoint>();

builder.Services.AddScoped<IDevicesEntrypoint, DevicesEntrypoint>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapWolverineEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference();

app.MapDefaultEndpoints();

app.Run();
