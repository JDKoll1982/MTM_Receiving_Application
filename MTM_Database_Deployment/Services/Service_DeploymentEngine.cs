using System.Diagnostics;
using MTM_Database_Deployment.Models;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Applies a deployment plan. Two modes are supported:
///  - UpdateInPlace: never drops data; adds missing tables/columns/indexes and refreshes code objects.
///  - FreshRebuild: drops and recreates the database, then deploys every object in order.
/// </summary>
public sealed class Service_DeploymentEngine
{
    private readonly Service_MySqlGateway _gateway;
    private readonly Service_ObjectCatalog _catalog;

    public Service_DeploymentEngine(Service_MySqlGateway gateway, Service_ObjectCatalog catalog)
    {
        _gateway = gateway;
        _catalog = catalog;
    }

    public async Task<Model_DeploymentResult> ApplyAsync(
        Model_DeploymentPlan plan,
        bool dropConfirmedExtras,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new Model_DeploymentResult();
        var objects = _catalog.LoadObjects();

        await _gateway.ResolveServerAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(_gateway.ResolutionMessage);

        // 1. Create any missing tables first so later alterations can reference them.
        var schemaObjects = objects.Where(item => item.Kind == Enum_SqlObjectKind.Schema).ToList();
        foreach (var schemaObject in schemaObjects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"Ensuring table: {schemaObject.RelativePath}");

            var script = await File.ReadAllTextAsync(schemaObject.FullPath, cancellationToken).ConfigureAwait(false);
            try
            {
                await _gateway.ExecuteStatementsAsync(
                    plan.Database,
                    Service_SqlScriptParser.SplitStatements(script),
                    continueOnError: false,
                    onError: null,
                    disableForeignKeyChecks: true,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                result.MarkFailed($"{schemaObject.RelativePath}: {exception.Message}");
                throw;
            }
        }

        result.MarkApplied($"{schemaObjects.Count} table script(s) ensured.");

        // 2. Apply column, index, and foreign key changes computed by the comparer.
        foreach (var change in plan.Changes.Where(item => item.Kind is Enum_ChangeKind.AddColumn
                     or Enum_ChangeKind.AlterColumn or Enum_ChangeKind.AddIndex or Enum_ChangeKind.AddForeignKey))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"{change.KindLabel}: {change.Target}");

            foreach (var statement in Service_SqlScriptParser.SplitStatements(change.Sql ?? string.Empty))
            {
                try
                {
                    await _gateway.ExecuteAsync(plan.Database, statement, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception) when (IsBenign(exception))
                {
                    result.MarkBenignSkip($"{change.KindLabel} {change.Target}");
                }
                catch (Exception exception)
                {
                    result.MarkFailed($"{change.KindLabel} {change.Target}: {exception.Message}");
                }
            }

            result.MarkApplied($"{change.KindLabel}: {change.Target}");
        }

        // 3. Refresh views, procedures, and triggers.
        foreach (var kind in new[] { Enum_SqlObjectKind.View, Enum_SqlObjectKind.Procedure, Enum_SqlObjectKind.Trigger })
        {
            foreach (var sqlObject in objects.Where(item => item.Kind == kind))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report($"Deploying: {sqlObject.RelativePath}");

                var script = await File.ReadAllTextAsync(sqlObject.FullPath, cancellationToken).ConfigureAwait(false);
                var errors = new List<string>();

                await _gateway.ExecuteStatementsAsync(
                    plan.Database,
                    Service_SqlScriptParser.SplitStatements(script),
                    continueOnError: true,
                    onError: errors.Add,
                    disableForeignKeyChecks: false,
                    cancellationToken).ConfigureAwait(false);

                foreach (var error in errors)
                {
                    if (IsBenignMessage(error))
                    {
                        result.MarkBenignSkip(sqlObject.RelativePath);
                    }
                    else
                    {
                        result.MarkFailed($"{sqlObject.RelativePath}: {error}");
                    }
                }

                if (errors.Count == 0)
                {
                    result.MarkApplied(sqlObject.RelativePath);
                }
            }
        }

        // 4. Seed data (idempotent; duplicates are expected and reported as already applied).
        foreach (var seed in objects.Where(item => item.Kind == Enum_SqlObjectKind.Seed))
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"Seeding: {seed.RelativePath}");

            var script = await File.ReadAllTextAsync(seed.FullPath, cancellationToken).ConfigureAwait(false);
            var errors = new List<string>();

            await _gateway.ExecuteStatementsAsync(
                plan.Database,
                Service_SqlScriptParser.SplitStatements(script),
                continueOnError: true,
                onError: errors.Add,
                disableForeignKeyChecks: false,
                cancellationToken).ConfigureAwait(false);

            foreach (var error in errors)
            {
                if (IsBenignMessage(error))
                {
                    result.MarkBenignSkip(seed.RelativePath);
                }
                else
                {
                    result.MarkFailed($"{seed.RelativePath}: {error}");
                }
            }

            if (errors.Count == 0)
            {
                result.MarkApplied(seed.RelativePath);
            }
        }

        // 5. Confirmed removals last, so a failed deploy never destroys anything early.
        if (dropConfirmedExtras)
        {
            foreach (var change in plan.Changes.Where(item => item.IsRemoval && !string.IsNullOrWhiteSpace(item.Sql)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report($"Removing: {change.Target}");

                try
                {
                    await _gateway.ExecuteAsync(plan.Database, change.Sql!, cancellationToken).ConfigureAwait(false);
                    result.MarkApplied($"Removed {change.KindLabel}: {change.Target}");
                }
                catch (Exception exception)
                {
                    result.MarkFailed($"Remove {change.Target}: {exception.Message}");
                }
            }
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        return result;
    }

    public async Task<Model_DeploymentResult> RebuildAsync(
        string database,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new Model_DeploymentResult();

        await _gateway.ResolveServerAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(_gateway.ResolutionMessage);
        progress?.Report($"Dropping and recreating {database}...");

        await _gateway.ExecuteWithoutDatabaseAsync(
            $"DROP DATABASE IF EXISTS `{database}`; CREATE DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
            cancellationToken).ConfigureAwait(false);

        var objects = _catalog.LoadObjects();
        foreach (var kind in new[] { Enum_SqlObjectKind.Schema, Enum_SqlObjectKind.View, Enum_SqlObjectKind.Procedure, Enum_SqlObjectKind.Trigger, Enum_SqlObjectKind.Seed })
        {
            foreach (var sqlObject in objects.Where(item => item.Kind == kind))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report($"Deploying: {sqlObject.RelativePath}");

                var script = await File.ReadAllTextAsync(sqlObject.FullPath, cancellationToken).ConfigureAwait(false);
                var errors = new List<string>();

                await _gateway.ExecuteStatementsAsync(
                    database,
                    Service_SqlScriptParser.SplitStatements(script),
                    continueOnError: true,
                    onError: errors.Add,
                    disableForeignKeyChecks: kind == Enum_SqlObjectKind.Schema,
                    cancellationToken).ConfigureAwait(false);

                foreach (var error in errors)
                {
                    if (IsBenignMessage(error))
                    {
                        result.MarkBenignSkip(sqlObject.RelativePath);
                    }
                    else
                    {
                        result.MarkFailed($"{sqlObject.RelativePath}: {error}");
                    }
                }

                if (errors.Count == 0)
                {
                    result.MarkApplied(sqlObject.RelativePath);
                }
            }
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        return result;
    }

    private static bool IsBenign(Exception exception) => IsBenignMessage(exception.Message);

    private static bool IsBenignMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        var normalized = message.ToLowerInvariant();
        string[] benignPatterns =
        {
            "already exists",
            "duplicate column name",
            "duplicate key name",
            "duplicate foreign key constraint name",
            "multiple primary key defined",
            "check that column/key exists",
            "duplicate entry",
            "can't drop"
        };

        return benignPatterns.Any(normalized.Contains);
    }
}
