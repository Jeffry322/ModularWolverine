# Modular Monolith Structure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Restructure the .NET 10 Aspire solution into a correctly named modular monolith under src, with Devices and Telematics module layers, centralized NuGet versions, and in-process Wolverine messaging.

**Architecture:** The API remains the composition root and references each module's Application and Infrastructure projects. Within a module, Application references Domain and IntegrationEvents, while Infrastructure references Application and Domain; Domain and IntegrationEvents are independent. Wolverine is registered only in the API and scans both Application assemblies.

**Tech Stack:** .NET 10, ASP.NET Core, .NET Aspire 13.4.6, WolverineFx 6.20.0, Central Package Management, SLNX.

## Global Constraints

- All source projects live below src; module projects live below src/Modules.
- Correct ModualarWolverine to ModularWolverine in source paths, project names, namespaces, solution entries, and references.
- Use Wolverine only for local, in-process messaging; add no transport or persistence package.
- Declare every PackageReference version in repository-root Directory.Packages.props.
- Preserve existing Aspire, API, ServiceDefaults, and Web behavior.
- The workspace has no discoverable Git repository, so commit steps are unavailable.

---

### Task 1: Relocate and Correct the Existing Solution

**Files:**
- Create: Directory.Packages.props
- Create: ModularWolverine.slnx
- Move: the four ModualarWolverine.* project directories to corrected names under src
- Rename: all four moved project files from ModualarWolverine.*.csproj to ModularWolverine.*.csproj
- Remove after replacement: ModualarWolverine.slnx
- Modify: moved project references and source namespaces containing the old spelling

**Interfaces:**
- Consumes: the existing four-project Aspire solution.
- Produces: four corrected projects under src, root CPM configuration, and a corrected solution file.

- [ ] **Step 1: Establish the expected structural failure**

Run:

~~~bash
test -f ModularWolverine.slnx
test -f Directory.Packages.props
test -d src/ModularWolverine.ApiService
~~~

Expected: at least one non-zero exit because the layout does not exist.

- [ ] **Step 2: Move and rename the four complete project directories**

Create src. Move API, AppHost, ServiceDefaults, and Web into it while correcting both directory and csproj filenames. Move all owned configuration, launch settings, Razor components, static assets, and HTTP samples with each project.

- [ ] **Step 3: Create CPM configuration**

Create Directory.Packages.props:

~~~xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.8" />
    <PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="10.6.0" />
    <PackageVersion Include="Microsoft.Extensions.ServiceDiscovery" Version="10.6.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.15.3" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.15.3" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.15.2" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.15.1" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Runtime" Version="1.15.1" />
    <PackageVersion Include="WolverineFx" Version="6.20.0" />
  </ItemGroup>
</Project>
~~~

Remove Version attributes from moved PackageReference items. Keep Aspire.AppHost.Sdk/13.4.6 in the AppHost project because CPM does not manage MSBuild project SDK versions.

- [ ] **Step 4: Correct project references and source identifiers**

Update host ProjectReference paths for the corrected sibling directories. Change Web namespaces and using directives from ModualarWolverine.Web to ModularWolverine.Web. Change AppHost generated project identifiers to Projects.ModularWolverine_ApiService and Projects.ModularWolverine_Web.

- [ ] **Step 5: Create the corrected solution**

Create ModularWolverine.slnx:

~~~xml
<Solution>
  <Project Path="src/ModularWolverine.ApiService/ModularWolverine.ApiService.csproj" />
  <Project Path="src/ModularWolverine.AppHost/ModularWolverine.AppHost.csproj" />
  <Project Path="src/ModularWolverine.ServiceDefaults/ModularWolverine.ServiceDefaults.csproj" />
  <Project Path="src/ModularWolverine.Web/ModularWolverine.Web.csproj" />
</Solution>
~~~

Remove the obsolete ModualarWolverine.slnx after this replacement exists.

- [ ] **Step 6: Verify relocation and naming**

Run:

~~~bash
test -f ModularWolverine.slnx
test -f Directory.Packages.props
test -d src/ModularWolverine.ApiService
test ! -e ModualarWolverine.ApiService
test ! -e ModualarWolverine.slnx
! rg -n 'ModualarWolverine' src ModularWolverine.slnx --glob '!bin/**' --glob '!obj/**'
! rg -n '<PackageReference[^>]*Version=' src --glob '*.csproj'
~~~

Expected: every command exits zero.

- [ ] **Step 7: Commit checkpoint**

Unavailable: git status reports that this workspace is not a Git repository.

### Task 2: Add Devices and Telematics Module Projects

**Files:**
- Create: four ModularWolverine.Modules.Devices.* projects beneath src/Modules/Devices
- Create: four ModularWolverine.Modules.Telematics.* projects beneath src/Modules/Telematics
- Create: AssemblyReference.cs in each Application project
- Modify: src/ModularWolverine.ApiService/ModularWolverine.ApiService.csproj
- Modify: ModularWolverine.slnx

**Interfaces:**
- Consumes: corrected source layout and solution from Task 1.
- Produces: eight .NET 10 libraries, two public Application assembly markers, compiler-enforced intra-module dependencies, and API composition-root references.

- [ ] **Step 1: Establish the expected module failure**

Run:

~~~bash
test -f src/Modules/Devices/ModularWolverine.Modules.Devices.Application/ModularWolverine.Modules.Devices.Application.csproj
test -f src/Modules/Telematics/ModularWolverine.Modules.Telematics.Application/ModularWolverine.Modules.Telematics.Application.csproj
~~~

Expected: non-zero exit because the module projects do not exist.

- [ ] **Step 2: Create dependency-free Domain and IntegrationEvents projects**

Create the Domain and IntegrationEvents project for each module with:

~~~xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
~~~

The project filename supplies its matching assembly name and root namespace.

- [ ] **Step 3: Create Application projects**

The Devices Application project contains:

~~~xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ModularWolverine.Modules.Devices.Domain\ModularWolverine.Modules.Devices.Domain.csproj" />
    <ProjectReference Include="..\ModularWolverine.Modules.Devices.IntegrationEvents\ModularWolverine.Modules.Devices.IntegrationEvents.csproj" />
  </ItemGroup>
</Project>
~~~

Create the exact Telematics equivalent by consistently substituting Telematics for Devices.

- [ ] **Step 4: Create Infrastructure projects**

Use the same .NET 10 property group. The Devices Infrastructure ItemGroup is:

~~~xml
<ItemGroup>
  <ProjectReference Include="..\ModularWolverine.Modules.Devices.Application\ModularWolverine.Modules.Devices.Application.csproj" />
  <ProjectReference Include="..\ModularWolverine.Modules.Devices.Domain\ModularWolverine.Modules.Devices.Domain.csproj" />
</ItemGroup>
~~~

Create the exact Telematics equivalent.

- [ ] **Step 5: Add Application assembly markers**

Devices AssemblyReference.cs:

~~~csharp
namespace ModularWolverine.Modules.Devices.Application;

public static class AssemblyReference
{
}
~~~

Create the corresponding public static class in namespace ModularWolverine.Modules.Telematics.Application.

- [ ] **Step 6: Add module references to API and solution**

Reference both Application and both Infrastructure projects from the API. Add all eight projects to ModularWolverine.slnx using paths beneath src/Modules/Devices and src/Modules/Telematics.

- [ ] **Step 7: Verify the module graph**

Run:

~~~bash
find src/Modules -name '*.csproj' -print | sort
dotnet sln ModularWolverine.slnx list
~~~

Expected: exactly eight module project files and twelve solution projects.

- [ ] **Step 8: Commit checkpoint**

Unavailable: git status reports that this workspace is not a Git repository.

### Task 3: Register Wolverine and Verify the Solution

**Files:**
- Modify: src/ModularWolverine.ApiService/ModularWolverine.ApiService.csproj
- Modify: src/ModularWolverine.ApiService/Program.cs

**Interfaces:**
- Consumes: both module Application.AssemblyReference types from Task 2.
- Produces: an API host using Wolverine core with explicit discovery of both Application assemblies.

- [ ] **Step 1: Establish the expected Wolverine failure**

Run:

~~~bash
rg -n 'PackageReference Include="WolverineFx"' src/ModularWolverine.ApiService/ModularWolverine.ApiService.csproj
rg -n 'UseWolverine' src/ModularWolverine.ApiService/Program.cs
~~~

Expected: non-zero exit because Wolverine is not referenced or registered.

- [ ] **Step 2: Reference Wolverine**

Add this centrally versioned reference to the API project:

~~~xml
<PackageReference Include="WolverineFx" />
~~~

- [ ] **Step 3: Register Wolverine and handler discovery**

Add to Program.cs:

~~~csharp
using DevicesApplication = ModularWolverine.Modules.Devices.Application;
using TelematicsApplication = ModularWolverine.Modules.Telematics.Application;
using Wolverine;
~~~

Immediately after creating the builder, add:

~~~csharp
builder.Host.UseWolverine(options =>
{
    options.Discovery.IncludeAssembly(typeof(DevicesApplication.AssemblyReference).Assembly);
    options.Discovery.IncludeAssembly(typeof(TelematicsApplication.AssemblyReference).Assembly);
});
~~~

Do not configure transports, endpoints, persistence, or durability.

- [ ] **Step 4: Restore**

Run:

~~~bash
dotnet restore ModularWolverine.slnx
~~~

Expected: restore succeeds for all twelve projects.

- [ ] **Step 5: Build**

Run:

~~~bash
dotnet build ModularWolverine.slnx --no-restore
~~~

Expected: build succeeds with zero errors.

- [ ] **Step 6: Run final structural checks**

Run equivalent assertions that prove:
- exactly eight module csproj files exist;
- the solution lists twelve projects;
- source and solution contain no ModualarWolverine typo;
- no csproj PackageReference has a Version attribute;
- source and CPM files contain no RabbitMQ, AzureServiceBus, Kafka, or UseDurable configuration.

Search for test projects. If none exist, report that explicitly rather than claiming tests ran.

- [ ] **Step 7: Commit checkpoint**

Unavailable: git status reports that this workspace is not a Git repository.
