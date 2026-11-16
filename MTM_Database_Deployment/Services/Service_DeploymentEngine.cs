using System.Diagnostics;
using MTM_Database_Deployment.Models;
using MySql.Data.MySqlClient;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Applies a deployment plan. Two modes are supported:
///  - UpdateInPlace: never drops data; adds missing tables/columns/indexes and refreshes code objects.
///  - FreshRebuild: drops and recreates the database, then deploys every object in order.
/// </summary>
/// <remarks>
/// Both modes copy the existing database to <c>{database}_backup</c> before the first write. A
/// backup that cannot be taken stops the run, so no write ever lands on an unbacked database.
/// </remarks>
public sealed class Service_DeploymentEngine
{
    private readonly Service_MySqlGateway _gateway;
    private readonly Service_ObjectCatalog _catalog;
    private readonly Service_DatabaseBackup _backup;
    private readonly Service_SeedLedger? _seedLedger;

    public Service_DeploymentEngine(Service_MySqlGateway gateway, Service_ObjectCatalog catalog)
        : this(gateway, catalog, seedLedger: null)
    {
    }

    /// <param name="seedLedger">
    /// When supplied, a safe update skips seed scripts this database has already taken. Without it
    /// every seed runs, which is what a fresh rebuild needs.
    /// </param>
    public Service_DeploymentEngine(
        Service_MySqlGateway gateway,
        Service_ObjectCatalog catalog,
        Service_SeedLedger? seedLedger)
    {
        _gateway = gateway;
        _catalog = catalog;
        _backup = new Service_DatabaseBackup(gateway);
        _seedLedger = seedLedger;
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

        // 0. Snapshot the old database before anything is written.
        await BackupAsync(plan.Database, progress, result, cancellationToken).ConfigureAwait(false);

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

            var failed = false;
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
                    if (IsOrphanedRowViolation(exception)
                        && await TryWithForeignKeyChecksOffAsync(plan.Database, statement, cancellationToken).ConfigureAwait(false))
                    {
                        result.AddMessage(
                            $"{change.KindLabel} {change.Target}: attached with foreign key checks off - the table "
                            + "already holds rows whose parent record is missing, so the constraint is not validated.");
                        continue;
                    }

                    failed = true;
                    result.MarkFailed($"{change.KindLabel} {change.Target}: {exception.Message}");
                }
            }

            if (!failed)
            {
                result.MarkApplied($"{change.KindLabel}: {change.Target}");
            }
        }

        // 2b. Blocking removals run before anything writes. A database-only NOT NULL column with no
        // default fails every insert that omits it, so a confirmed cleanup must land in this run
        // rather than after the seeds it would otherwise break.
        await ApplyRemovalsAsync(plan, dropConfirmedExtras, blocking: true, progress, result, cancellationToken)
            .ConfigureAwait(false);

        // A column that could not be added or altered invalidates every view, procedure, and seed
        // that depends on it. Stopping here keeps the real error visible instead of burying it under
        // a cascade of downstream failures.
        if (result.FailedCount > 0)
        {
            result.AddMessage(
                $"Stopped after schema reconciliation: {result.FailedCount} change(s) failed, so views, procedures, "
                + "triggers, seeds, and the remaining removals were not run.");
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            return result;
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

        // 4. Seed data. A seed whose text has not changed since it last ran on this database is left
        // alone, so a safe update never writes reference data a second time.
        var seedScope = Service_SeedLedger.ScopeFor(_gateway.ActiveServer, _gateway.Port, plan.Database);
        foreach (var seed in objects.Where(item => item.Kind == Enum_SqlObjectKind.Seed))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var script = await File.ReadAllTextAsync(seed.FullPath, cancellationToken).ConfigureAwait(false);
            var fingerprint = Service_SeedLedger.Fingerprint(script);

            if (_seedLedger?.WasApplied(seedScope, seed.RelativePath, fingerprint) == true)
            {
                progress?.Report($"Already seeded: {seed.RelativePath}");
                result.MarkAlreadySeeded(seed.RelativePath);
                continue;
            }

            progress?.Report($"Seeding: {seed.RelativePath}");

            var errors = new List<string>();
            var failuresBefore = result.FailedCount;

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

            if (result.FailedCount == failuresBefore)
            {
                _seedLedger?.Record(seedScope, seed.RelativePath, fingerprint);
            }
        }

        // 5. Remaining removals last, so a failed deploy never destroys anything early.
        await ApplyRemovalsAsync(plan, dropConfirmedExtras, blocking: false, progress, result, cancellationToken)
            .ConfigureAwait(false);

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        return result;
    }

    /// <summary>
    /// Copies the database into <c>{database}_backup</c> and folds the outcome into the run report.
    /// Throws when the copy fails, which keeps the caller from writing anything.
    /// </summary>
    private async Task BackupAsync(
        string database,
        IProgress<string>? progress,
        Model_DeploymentResult result,
        CancellationToken cancellationToken)
    {
        var backup = await _backup
            .BackupAsync(database, progress, cancellationToken)
            .ConfigureAwait(false);

        foreach (var message in backup.Messages)
        {
            result.AddMessage($"Backup: {message}");
        }
    }

    private async Task ApplyRemovalsAsync(
        Model_DeploymentPlan plan,
        bool dropConfirmedExtras,
        bool blocking,
        IProgress<string>? progress,
        Model_DeploymentResult result,
        CancellationToken cancellationToken)
    {
        if (!dropConfirmedExtras)
        {
            return;
        }

        foreach (var change in plan.Changes.Where(item =>
                     item.IsRemoval && item.IsBlocking == blocking && !string.IsNullOrWhiteSpace(item.Sql)))
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

    public async Task<Model_DeploymentResult> RebuildAsync(
        string database,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new Model_DeploymentResult();

        await _gateway.ResolveServerAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(_gateway.ResolutionMessage);

        // The snapshot is taken before the drop, otherwise there is nothing left to copy.
        await BackupAsync(database, progress, result, cancellationToken).ConfigureAwait(false);

        progress?.Report($"Dropping and recreating {database}...");

        await _gateway.ExecuteWithoutDatabaseAsync(
            $"DROP DATABASE IF EXISTS `{database}`; CREATE DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
            cancellationToken).ConfigureAwait(false);

        var objects = _catalog.LoadObjects();

        // A rebuild starts from an empty database, so every seed has to run. Recording the
        // fingerprints afterwards keeps the next safe update from writing them again.
        var seedScope = Service_SeedLedger.ScopeFor(_gateway.ActiveServer, _gateway.Port, database);
        foreach (var kind in new[] { Enum_SqlObjectKind.Schema, Enum_SqlObjectKind.View, Enum_SqlObjectKind.Procedure, Enum_SqlObjectKind.Trigger, Enum_SqlObjectKind.Seed })
        {
            foreach (var sqlObject in objects.Where(item => item.Kind == kind))
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report($"Deploying: {sqlObject.RelativePath}");

                var script = await File.ReadAllTextAsync(sqlObject.FullPath, cancellationToken).ConfigureAwait(false);
                var errors = new List<string>();
                var failuresBefore = result.FailedCount;

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

                if (kind == Enum_SqlObjectKind.Seed && result.FailedCount == failuresBefore)
                {
                    _seedLedger?.Record(
                        seedScope,
                        sqlObject.RelativePath,
                        Service_SeedLedger.Fingerprint(script));
                }
            }
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        return result;
    }

    private static bool IsBenign(Exception exception) => IsBenignMessage(exception.Message);

    /// <summary>
    /// True for MySQL 1452: a foreign key cannot be validated because rows already point at a parent
    /// that does not exist. Live databases here reach that state through seeds that run with foreign
    /// key checks off, so the constraint has to be re-attached the same way.
    /// </summary>
    private static bool IsOrphanedRowViolation(Exception exception)
    {
        if (exception is MySqlException mysqlException && mysqlException.Number == 1452)
        {
            return true;
        }

        return exception.Message.Contains("Cannot add or update a child row", StringComparison.OrdinalIgnoreCase)
               && exception.Message.Contains("foreign key constraint fails", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> TryWithForeignKeyChecksOffAsync(
        string database,
        string statement,
        CancellationToken cancellationToken)
    {
        try
        {
            await _gateway
                .ExecuteIgnoringForeignKeyChecksAsync(database, statement, cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

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
