var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ModularWolverine_ApiService>("apiservice").WithHttpHealthCheck("/health");

var pgServer = builder
    .AddPostgres("application")
    .WithDataVolume()
    .WithPgAdmin();

var database = pgServer.AddDatabase("modular-wolverine");

builder.AddProject<Projects.ModularWolverine_ApiService>("webapi")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
