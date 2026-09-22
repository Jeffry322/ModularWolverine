using Microsoft.EntityFrameworkCore;
using ModularWolverine.ApiService;
using ModularWolverine.Modules.Devices.Infrastructure;
using ModularWolverine.Modules.Telematics.Infrastructure;
using ModularWolverine.Modules.Devices.Application;
using ModularWolverine.Modules.Devices.Application.Common.Contracts;
using ModularWolverine.Modules.Devices.Infrastructure.Database;
using ModularWolverine.Modules.Telematics.Application;
using ModularWolverine.Modules.Telematics.Application.Common.Contracts;
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
    
    options.UseEntityFrameworkCoreTransactions()
        .WithDbContextAbstraction<IDevicesDbContext, DevicesDbContext>();
    
    options.UseEntityFrameworkCoreTransactions()
        .WithDbContextAbstraction<ITelematicsDbContext, TelematicsDbContext>();

    options.PublishDomainEventsFromEntityFrameworkCore<Entity>(entity => entity.DomainEvents);
    
    options.CodeGeneration.Sources.Insert(
        0,
        new DevicesDbContextVariableSource());
});

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddWolverineHttp();

builder.Services.AddOpenApi();

builder.AddDevicesModule(builder);

builder.AddTelematicsModule(builder);

builder.Services.AddScoped<ITelematicsEntrypoint, TelematicsEntrypoint>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapWolverineEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    
    var devicesDbContext = services.GetRequiredService<DevicesDbContext>();
    var telematicsDbContext = services.GetRequiredService<TelematicsDbContext>();
    
    await devicesDbContext.Database.MigrateAsync();
    await telematicsDbContext.Database.MigrateAsync();
}

app.MapScalarApiReference();

app.MapDefaultEndpoints();

app.Run();
