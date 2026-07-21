# Aspire Project Folder Design

## Objective

Group the .NET Aspire-specific projects beneath a physical and Rider-visible `src/Aspire` directory without changing project, assembly, or namespace names.

## Target Layout

```text
src/
  Aspire/
    ModularWolverine.AppHost/
    ModularWolverine.ServiceDefaults/
  ModularWolverine.ApiService/
  ModularWolverine.Web/
  Modules/
    Devices/
    Telematics/
```

The SLNX hierarchy mirrors this layout with an explicit `/src/Aspire/` solution folder.

## Path Changes

- Move `src/ModularWolverine.AppHost` to `src/Aspire/ModularWolverine.AppHost`.
- Move `src/ModularWolverine.ServiceDefaults` to `src/Aspire/ModularWolverine.ServiceDefaults`.
- Update AppHost references to the API and Web projects for the additional directory depth.
- Update API and Web references to ServiceDefaults through `src/Aspire`.
- Update `ModularWolverine.slnx` project paths and group both projects beneath `/src/Aspire/`.
- Update `aspire.config.json` to point to the relocated AppHost project.

Project filenames, root namespaces, assembly names, Aspire resource names, and runtime behavior remain unchanged.

## Verification

1. Both projects exist beneath `src/Aspire`, and their previous directories do not exist.
2. Rider's SLNX hierarchy contains `/src/Aspire/` with AppHost and ServiceDefaults.
3. The solution still lists twelve projects.
4. No project or configuration file references the previous project locations.
5. Restore and a single-node solution build succeed.
6. The corrected solution is reopened in Rider to refresh its project model.

## Out of Scope

- Renaming AppHost or ServiceDefaults
- Changing Aspire topology or runtime behavior
- Moving API, Web, or module projects
- Updating package versions

