# Aspire Project Folder Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Relocate AppHost and ServiceDefaults beneath `src/Aspire` while preserving all project identities and runtime behavior.

**Architecture:** `src/Aspire` becomes the physical and Rider-visible home for Aspire orchestration and shared service defaults. Only filesystem and reference paths change; project names, namespaces, assemblies, and application behavior stay unchanged.

**Tech Stack:** .NET 10, .NET Aspire 13.4.6, SLNX, Rider.

## Global Constraints

- Move only AppHost and ServiceDefaults.
- Preserve project filenames, assembly names, namespaces, Aspire resource names, and runtime behavior.
- Keep API, Web, and module projects in their current locations.
- Do not update package versions.
- Use single-node MSBuild verification because parallel builds fail silently in this environment.
- Git commits are unavailable because this workspace is not a Git repository.

---

### Task 1: Relocate Aspire Projects and Refresh Rider

**Files:**
- Move: `src/ModularWolverine.AppHost/**` to `src/Aspire/ModularWolverine.AppHost/**`
- Move: `src/ModularWolverine.ServiceDefaults/**` to `src/Aspire/ModularWolverine.ServiceDefaults/**`
- Modify: `src/Aspire/ModularWolverine.AppHost/ModularWolverine.AppHost.csproj`
- Modify: `src/ModularWolverine.ApiService/ModularWolverine.ApiService.csproj`
- Modify: `src/ModularWolverine.Web/ModularWolverine.Web.csproj`
- Modify: `ModularWolverine.slnx`
- Modify: `aspire.config.json`

**Interfaces:**
- Consumes: the existing twelve-project solution with AppHost and ServiceDefaults directly beneath `src`.
- Produces: the same twelve-project solution with both Aspire projects beneath `src/Aspire` and Rider solution folder `/src/Aspire/`.

- [ ] **Step 1: Verify the target layout is initially absent**

Run:

~~~bash
test -d src/Aspire
rg -n '<Folder Name="/src/Aspire/">' ModularWolverine.slnx
~~~

Expected: both commands exit non-zero.

- [ ] **Step 2: Move both complete project directories**

Create `src/Aspire`, then move:

~~~text
src/ModularWolverine.AppHost
  -> src/Aspire/ModularWolverine.AppHost
src/ModularWolverine.ServiceDefaults
  -> src/Aspire/ModularWolverine.ServiceDefaults
~~~

- [ ] **Step 3: Update project references**

AppHost references become:

~~~xml
<ProjectReference Include="..\..\ModularWolverine.ApiService\ModularWolverine.ApiService.csproj" />
<ProjectReference Include="..\..\ModularWolverine.Web\ModularWolverine.Web.csproj" />
~~~

API and Web reference ServiceDefaults through:

~~~xml
<ProjectReference Include="..\Aspire\ModularWolverine.ServiceDefaults\ModularWolverine.ServiceDefaults.csproj" />
~~~

- [ ] **Step 4: Update the SLNX hierarchy**

Place AppHost and ServiceDefaults under:

~~~xml
<Folder Name="/src/Aspire/">
    <Project Path="src/Aspire/ModularWolverine.AppHost/ModularWolverine.AppHost.csproj" />
    <Project Path="src/Aspire/ModularWolverine.ServiceDefaults/ModularWolverine.ServiceDefaults.csproj" />
</Folder>
~~~

Keep API and Web beneath `/src/`, and keep all module solution folders unchanged.

- [ ] **Step 5: Update Aspire CLI configuration**

Set `aspire.config.json` to:

~~~json
{
  "appHost": {
    "path": "src/Aspire/ModularWolverine.AppHost/ModularWolverine.AppHost.csproj"
  }
}
~~~

- [ ] **Step 6: Verify paths and compile**

Run:

~~~bash
test -d src/Aspire/ModularWolverine.AppHost
test -d src/Aspire/ModularWolverine.ServiceDefaults
test ! -e src/ModularWolverine.AppHost
test ! -e src/ModularWolverine.ServiceDefaults
dotnet sln ModularWolverine.slnx list
dotnet restore ModularWolverine.slnx -m:1
dotnet build ModularWolverine.slnx --no-restore -m:1
~~~

Expected: the solution lists twelve projects using the new Aspire paths; restore and build exit zero.

- [ ] **Step 7: Refresh Rider**

Open `ModularWolverine.slnx` with the Rider launcher so the running IDE reloads the `src/Aspire` hierarchy.

- [ ] **Step 8: Commit checkpoint**

Unavailable: this workspace is not a Git repository.

