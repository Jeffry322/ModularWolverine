var builder = DistributedApplication.CreateBuilder(args);

var pgServer = builder
    .AddPostgres("application")
    .WithDataVolume()
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var database = pgServer.AddDatabase("modular-wolverine");

builder.AddProject<Projects.ModularWolverine_ApiService>("webapi")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithUrl("/scalar", "Scalar API");

builder.Build().Run();
