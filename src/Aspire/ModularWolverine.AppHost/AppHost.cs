var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ModularWolverine_ApiService>("apiservice").WithHttpHealthCheck("/health");

builder.AddProject<Projects.ModularWolverine_ApiService>("webapi")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
