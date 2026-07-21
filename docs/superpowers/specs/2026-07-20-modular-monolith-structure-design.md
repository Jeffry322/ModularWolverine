# Modular Monolith Structure Design

## Objective

Prepare the existing .NET 10 Aspire solution as a modular monolith that uses Wolverine for in-process messaging. Correct the `ModualarWolverine` typo throughout the solution, place all source projects under `src`, introduce Devices and Telematics module boundaries, and manage all NuGet package versions with Central Package Management (CPM).

## Repository Layout

The corrected solution file remains at the repository root:

```text
ModularWolverine.slnx
Directory.Packages.props
src/
  ModularWolverine.ApiService/
  ModularWolverine.AppHost/
  ModularWolverine.ServiceDefaults/
  ModularWolverine.Web/
  Modules/
    Devices/
      ModularWolverine.Modules.Devices.Application/
      ModularWolverine.Modules.Devices.Domain/
      ModularWolverine.Modules.Devices.Infrastructure/
      ModularWolverine.Modules.Devices.IntegrationEvents/
    Telematics/
      ModularWolverine.Modules.Telematics.Application/
      ModularWolverine.Modules.Telematics.Domain/
      ModularWolverine.Modules.Telematics.Infrastructure/
      ModularWolverine.Modules.Telematics.IntegrationEvents/
```

Each module layer is a .NET 10 class-library project. Project files, assembly names, root namespaces, source namespaces, solution entries, and project references use the corrected `ModularWolverine` spelling.

## Module Boundaries

Each module uses this dependency direction:

```text
IntegrationEvents     Domain
        ^                ^
        |                |
        +--- Application-+
                 ^
                 |
          Infrastructure
```

- `Domain` contains the module's domain model and has no project dependencies.
- `IntegrationEvents` contains public messages intended for communication outside the module and has no project dependencies.
- `Application` contains use cases and message handlers. It references its module's `Domain` and `IntegrationEvents` projects.
- `Infrastructure` contains technical implementations. It references its module's `Application` and `Domain` projects.
- `ModularWolverine.ApiService` is the composition root. It references both modules' `Application` and `Infrastructure` projects.
- Devices and Telematics do not reference one another directly. Future cross-module communication uses integration-event contracts.

The initial projects remain intentionally minimal; this preparation establishes boundaries without inventing domain behavior.

## Wolverine Messaging

`ModularWolverine.ApiService` references the core `WolverineFx` package and registers Wolverine with the application host. Wolverine is configured for local, in-process messaging only. The preparation does not add RabbitMQ, Azure Service Bus, Kafka, database-backed durability, or any other transport or persistence package.

Application assemblies are included in Wolverine handler discovery so handlers added to either module can be found without further composition-root restructuring.

## Central Package Management

The repository-root `Directory.Packages.props` sets `ManagePackageVersionsCentrally` to `true` and contains a `PackageVersion` item for every NuGet `PackageReference` used by source projects. Individual project files retain package identities but contain no `Version` attributes on `PackageReference` items.

The `Aspire.AppHost.Sdk` version is an MSBuild project SDK version, not a NuGet `PackageReference`; CPM does not manage it. Its existing SDK version remains attached to the AppHost project unless a separate SDK-version management mechanism becomes necessary.

## Existing Application Behavior

The move preserves the existing Aspire topology and sample behavior:

- AppHost starts the API service and Blazor web frontend.
- Both executable web projects continue to use ServiceDefaults.
- The web frontend continues to resolve and call the API through Aspire service discovery.
- Existing configuration, launch settings, Razor components, static assets, and HTTP request samples move with their owning projects.
- Relative project references and generated Aspire project identifiers are updated for the new names and locations.

## Verification

The restructuring is complete when all of the following are true:

1. `ModularWolverine.slnx` lists the four relocated host projects and all eight module class libraries.
2. No source path, project name, assembly/root namespace, project reference, or source identifier retains the `ModualarWolverine` typo.
3. Every source project is located under `src`, with module projects beneath `src/Modules`.
4. No `PackageReference` in a project file declares a version.
5. Wolverine is registered in the API without an external transport.
6. `dotnet restore ModularWolverine.slnx` succeeds.
7. `dotnet build ModularWolverine.slnx --no-restore` succeeds without errors.
8. Any discovered test projects pass; if none exist, the verification report states that explicitly.

## Out of Scope

- Domain entities, use cases, endpoints, or concrete integration events
- Message brokers and external transports
- Durable inbox/outbox storage
- Module-specific databases or migrations
- New UI behavior
- Deployment changes beyond correcting relocated project paths
