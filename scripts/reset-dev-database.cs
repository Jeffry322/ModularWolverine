using System.Diagnostics;

return await ResetDatabaseAsync(args);

static async Task<int> ResetDatabaseAsync(string[] args)
{
    const string databaseName = "modular-wolverine";

    if (args.Contains("--help"))
    {
        Console.WriteLine("""
            Rebuild the ModularWolverine development database from fresh initial migrations.

            The command will:
              1. Delete every module's existing migration files.
              2. Generate a fresh Initial<Module> migration from the current model.
              3. Drop and recreate the development database.
              4. Apply every newly generated module migration.

            Usage:
              dotnet run scripts/reset-dev-database.cs -- [--dry-run] [--force]

            Options:
              --dry-run  Validate the projects and DbContexts without changing files or the database.
              --force    Skip the database-name confirmation.
            """);
        return 0;
    }

    var unknownArguments = args.Except(["--dry-run", "--force"]).ToArray();
    if (unknownArguments.Length > 0)
    {
        Console.Error.WriteLine($"Unknown argument: {unknownArguments[0]}");
        return 1;
    }

    try
    {
        var repositoryRoot = FindRepositoryRoot();
        var modules = DiscoverModules(repositoryRoot);
        var container = await FindDatabaseContainerAsync(repositoryRoot);
        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"modular-wolverine-reset-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            await RunCheckedAsync("dotnet", ["tool", "restore"], repositoryRoot);
            await RunCheckedAsync(
                "dotnet",
                ["build", "ModularWolverine.slnx", "--nologo"],
                repositoryRoot);

            Console.WriteLine($"Container: {container.Name} ({container.Image})");
            Console.WriteLine($"Database:  {databaseName}");
            Console.WriteLine($"Modules:   {string.Join(", ", modules.Select(module => module.Name))}");

            if (args.Contains("--dry-run"))
            {
                foreach (var module in modules)
                {
                    await RunCheckedAsync(
                        "dotnet",
                        [
                            "ef", "dbcontext", "info",
                            "--no-build",
                            "--context", module.Context,
                            "--project", module.ProjectPath,
                            "--startup-project", ApiProjectPath(repositoryRoot)
                        ],
                        repositoryRoot);
                }

                Console.WriteLine($"Dry run succeeded for {modules.Count} module(s). No files or database objects were changed.");
                return 0;
            }

            if (!args.Contains("--force"))
            {
                var confirmation = $"reset {databaseName} in {container.Name}";
                Console.Write($"Type '{confirmation}' to delete all migrations and rebuild the database: ");
                if (!string.Equals(Console.ReadLine(), confirmation, StringComparison.Ordinal))
                {
                    Console.WriteLine("Reset cancelled.");
                    return 0;
                }
            }

            var backupDirectory = Path.Combine(
                repositoryRoot,
                $".migration-reset-backup-{Guid.NewGuid():N}");
            var generatedScripts = new List<(ModuleMigration Module, string Path)>();
            var migrationsBackedUp = false;
            var databaseDropped = false;

            try
            {
                BackupMigrations(modules, backupDirectory);
                migrationsBackedUp = true;

                foreach (var module in modules)
                {
                    await RunCheckedAsync(
                        "dotnet",
                        [
                            "ef", "migrations", "add", $"Initial{module.Name}",
                            "--context", module.Context,
                            "--project", module.ProjectPath,
                            "--startup-project", ApiProjectPath(repositoryRoot),
                            "--output-dir", "Database/Migrations"
                        ],
                        repositoryRoot);
                }

                await RunCheckedAsync(
                    "dotnet",
                    ["build", "ModularWolverine.slnx", "--no-restore", "--nologo"],
                    repositoryRoot);

                foreach (var module in modules)
                {
                    var scriptPath = Path.Combine(temporaryDirectory, $"{module.Name}.sql");
                    await RunCheckedAsync(
                        "dotnet",
                        [
                            "ef", "migrations", "script",
                            "--no-build",
                            "--context", module.Context,
                            "--project", module.ProjectPath,
                            "--startup-project", ApiProjectPath(repositoryRoot),
                            "--output", scriptPath
                        ],
                        repositoryRoot);

                    generatedScripts.Add((module, scriptPath));
                }

                await RunCheckedAsync(
                    "docker",
                    [
                        "exec", container.Id, "sh", "-lc",
                        "export PGPASSWORD=\"$POSTGRES_PASSWORD\"; " +
                        "exec psql -X -v ON_ERROR_STOP=1 -U \"$POSTGRES_USER\" -d postgres -c \"SELECT 1\""
                    ],
                    repositoryRoot);

                await RunCheckedAsync(
                    "docker",
                    [
                        "exec", container.Id, "sh", "-lc",
                        "export PGPASSWORD=\"$POSTGRES_PASSWORD\"; " +
                        $"exec dropdb --if-exists --force -U \"$POSTGRES_USER\" {databaseName}"
                    ],
                    repositoryRoot);
                databaseDropped = true;

                await RunCheckedAsync(
                    "docker",
                    [
                        "exec", container.Id, "sh", "-lc",
                        "export PGPASSWORD=\"$POSTGRES_PASSWORD\"; " +
                        $"exec createdb -U \"$POSTGRES_USER\" {databaseName}"
                    ],
                    repositoryRoot);

                foreach (var (module, scriptPath) in generatedScripts)
                {
                    Console.WriteLine($"Applying {module.Name} migrations...");
                    await RunCheckedAsync(
                        "docker",
                        [
                            "exec", "-i", container.Id, "sh", "-lc",
                            "export PGPASSWORD=\"$POSTGRES_PASSWORD\"; " +
                            $"exec psql -X -v ON_ERROR_STOP=1 -U \"$POSTGRES_USER\" -d {databaseName}"
                        ],
                        repositoryRoot,
                        scriptPath);
                }

                await VerifyAppliedMigrationsAsync(
                    modules,
                    container,
                    databaseName,
                    repositoryRoot);

                Directory.Delete(backupDirectory, true);
            }
            catch
            {
                if (migrationsBackedUp && !databaseDropped)
                {
                    RestoreMigrations(modules, backupDirectory);
                }
                else if (Directory.Exists(backupDirectory))
                {
                    Console.Error.WriteLine($"Previous migrations were retained at '{backupDirectory}'.");
                }

                throw;
            }

            Console.WriteLine($"Fresh initial migrations and development database '{databaseName}' were created successfully.");
            Console.WriteLine("Wolverine will recreate its own persistence objects when the API next starts.");
            return 0;
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Reset failed: {exception.Message}");
        return 1;
    }
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Environment.CurrentDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "ModularWolverine.slnx")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("Run this command from the ModularWolverine repository.");
}

static IReadOnlyList<ModuleMigration> DiscoverModules(string repositoryRoot)
{
    var modulesDirectory = Path.Combine(repositoryRoot, "src", "Modules");
    var modules = new List<ModuleMigration>();

    foreach (var moduleDirectory in Directory.EnumerateDirectories(modulesDirectory).Order())
    {
        var moduleName = Path.GetFileName(moduleDirectory);
        if (moduleName.Length == 0 || !moduleName.All(char.IsLetterOrDigit))
        {
            throw new InvalidOperationException(
                $"Module directory '{moduleName}' must use only letters and digits.");
        }

        var infrastructureProjects = Directory
            .EnumerateFiles(moduleDirectory, "*.Infrastructure.csproj", SearchOption.AllDirectories)
            .ToArray();

        if (infrastructureProjects.Length != 1)
        {
            throw new InvalidOperationException(
                $"Module '{moduleName}' must contain exactly one Infrastructure project; found {infrastructureProjects.Length}.");
        }

        var projectPath = infrastructureProjects[0];
        modules.Add(new ModuleMigration(
            moduleName,
            $"{moduleName}DbContext",
            projectPath,
            Path.Combine(Path.GetDirectoryName(projectPath)!, "Database", "Migrations"),
            moduleName.ToLowerInvariant()));
    }

    if (modules.Count == 0)
    {
        throw new InvalidOperationException("No modules were found under src/Modules.");
    }

    return modules;
}

static async Task<DatabaseContainer> FindDatabaseContainerAsync(string repositoryRoot)
{
    var result = await RunCommandAsync(
        "docker",
        ["ps", "--filter", "name=^/application-", "--format", "{{.ID}}|{{.Names}}|{{.Image}}|{{.Networks}}"],
        repositoryRoot);
    EnsureSuccess(result, "Locating the Aspire PostgreSQL container");

    var candidates = result.StandardOutput
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(line => line.Split('|', 4))
        .Where(parts =>
            parts.Length == 4 &&
            parts[2].StartsWith("postgres:", StringComparison.OrdinalIgnoreCase) &&
            parts[3]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(network =>
                    network.StartsWith("aspire-persistent-network-", StringComparison.Ordinal) &&
                    network.EndsWith("-ModularWolverine", StringComparison.Ordinal)))
        .ToArray();

    var containers = new List<DatabaseContainer>();
    foreach (var candidate in candidates)
    {
        var mountsResult = await RunCommandAsync(
            "docker",
            ["inspect", "--format", "{{range .Mounts}}{{println .Name}}{{end}}", candidate[0]],
            repositoryRoot);
        EnsureSuccess(mountsResult, $"Inspecting container '{candidate[1]}'");

        var hasExpectedDataVolume = mountsResult.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(volume =>
                volume.StartsWith("modularwolverine.apphost-", StringComparison.Ordinal) &&
                volume.EndsWith("-application-data", StringComparison.Ordinal));

        if (hasExpectedDataVolume)
        {
            containers.Add(new DatabaseContainer(candidate[0], candidate[1], candidate[2]));
        }
    }

    return containers.Count switch
    {
        1 => containers[0],
        0 => throw new InvalidOperationException(
            "No ModularWolverine Aspire PostgreSQL container is running. Start the AppHost first."),
        _ => throw new InvalidOperationException(
            $"Expected one ModularWolverine Aspire PostgreSQL container, but found {containers.Count}.")
    };
}

static string ApiProjectPath(string repositoryRoot)
{
    return Path.Combine(
        repositoryRoot,
        "src/ModularWolverine.ApiService/ModularWolverine.ApiService.csproj");
}

static void BackupMigrations(
    IEnumerable<ModuleMigration> modules,
    string backupDirectory)
{
    Directory.CreateDirectory(backupDirectory);
    var movedMigrations = new Stack<(string Source, string Backup)>();

    try
    {
        foreach (var module in modules)
        {
            if (!Directory.Exists(module.MigrationsDirectory))
            {
                continue;
            }

            var backupPath = Path.Combine(backupDirectory, module.Name);
            Directory.Move(module.MigrationsDirectory, backupPath);
            movedMigrations.Push((module.MigrationsDirectory, backupPath));
        }
    }
    catch
    {
        foreach (var (source, backup) in movedMigrations)
        {
            Directory.Move(backup, source);
        }

        Directory.Delete(backupDirectory, true);
        throw;
    }
}

static void RestoreMigrations(
    IEnumerable<ModuleMigration> modules,
    string backupDirectory)
{
    foreach (var module in modules)
    {
        if (Directory.Exists(module.MigrationsDirectory))
        {
            Directory.Delete(module.MigrationsDirectory, true);
        }

        var backupPath = Path.Combine(backupDirectory, module.Name);
        if (Directory.Exists(backupPath))
        {
            Directory.Move(backupPath, module.MigrationsDirectory);
        }
    }

    if (Directory.Exists(backupDirectory))
    {
        Directory.Delete(backupDirectory, true);
    }
}

static async Task VerifyAppliedMigrationsAsync(
    IEnumerable<ModuleMigration> modules,
    DatabaseContainer container,
    string databaseName,
    string repositoryRoot)
{
    foreach (var module in modules)
    {
        var migrationId = Directory
            .EnumerateFiles(module.MigrationsDirectory, $"*_Initial{module.Name}.cs")
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.Ordinal))
            .Select(Path.GetFileNameWithoutExtension)
            .Single();
        var result = await RunCommandAsync(
            "docker",
            [
                "exec", container.Id, "sh", "-lc",
                "export PGPASSWORD=\"$POSTGRES_PASSWORD\"; " +
                $"exec psql -X -v ON_ERROR_STOP=1 -U \"$POSTGRES_USER\" -d {databaseName} " +
                $"-tAc \"SELECT migration_id FROM {module.Schema}.\\\"__EFMigrationsHistory\\\" " +
                $"WHERE migration_id = '{migrationId}';\""
            ],
            repositoryRoot);
        EnsureSuccess(result, $"Verifying {module.Name} migrations");

        if (!string.Equals(result.StandardOutput.Trim(), migrationId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Migration '{migrationId}' was not recorded for module '{module.Name}'.");
        }

        Console.WriteLine($"Verified {module.Name}: {migrationId}");
    }
}

static async Task RunCheckedAsync(
    string fileName,
    IReadOnlyCollection<string> arguments,
    string workingDirectory,
    string? standardInputFile = null)
{
    Console.WriteLine($"> {fileName} {string.Join(' ', arguments.Select(FormatArgument))}");

    var result = await RunCommandAsync(
        fileName,
        arguments,
        workingDirectory,
        standardInputFile);

    if (!string.IsNullOrWhiteSpace(result.StandardOutput))
    {
        Console.WriteLine(result.StandardOutput.TrimEnd());
    }

    if (!string.IsNullOrWhiteSpace(result.StandardError))
    {
        Console.Error.WriteLine(result.StandardError.TrimEnd());
    }

    EnsureSuccess(result, $"Running {fileName}");
}

static async Task<CommandResult> RunCommandAsync(
    string fileName,
    IReadOnlyCollection<string> arguments,
    string workingDirectory,
    string? standardInputFile = null)
{
    var startInfo = new ProcessStartInfo(fileName)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = standardInputFile is not null,
        UseShellExecute = false
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException($"Could not start '{fileName}'.");
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));

    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();

    try
    {
        if (standardInputFile is not null)
        {
            await using var input = File.OpenRead(standardInputFile);
            await input.CopyToAsync(process.StandardInput.BaseStream, timeout.Token);
            process.StandardInput.Close();
        }

        await process.WaitForExitAsync(timeout.Token);
    }
    catch (OperationCanceledException)
    {
        process.Kill(true);
        await process.WaitForExitAsync();
        throw new TimeoutException($"'{fileName}' did not finish within five minutes.");
    }

    return new CommandResult(
        process.ExitCode,
        await standardOutput,
        await standardError);
}

static void EnsureSuccess(CommandResult result, string operation)
{
    if (result.ExitCode != 0)
    {
        throw new InvalidOperationException($"{operation} failed with exit code {result.ExitCode}.");
    }
}

static string FormatArgument(string argument)
{
    return argument.Any(char.IsWhiteSpace) ? $"\"{argument}\"" : argument;
}

sealed record ModuleMigration(
    string Name,
    string Context,
    string ProjectPath,
    string MigrationsDirectory,
    string Schema);

sealed record DatabaseContainer(string Id, string Name, string Image);
sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
