using ModularWolverine.Modules.Devices.Infrastructure;
using ModularWolverine.Modules.Telematics.Infrastructure;
using ModularWolverine.Modules.Devices.Application;
using ModularWolverine.Modules.Telematics.Application;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWolverine(options =>
{
    options.UseRuntimeCompilation();
    options.Discovery.IncludeAssembly(typeof(ModularWolverine.Modules.Devices.Application.AssemblyReference).Assembly);
    options.Discovery.IncludeAssembly(typeof(ModularWolverine.Modules.Telematics.Application.AssemblyReference).Assembly);
});

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.AddDevicesModule(builder);

builder.AddTelematicsModule(builder);

builder.Services.AddScoped<ITelematicsEntrypoint, TelematicsEntrypoint>();

builder.Services.AddScoped<IDevicesEntrypoint, DevicesEntrypoint>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast",
        () =>
        {
            var forecast = Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]))
                .ToArray();
            return forecast;
        })
    .WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
