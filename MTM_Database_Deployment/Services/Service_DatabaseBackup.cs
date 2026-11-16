using MTM_Database_Deployment.Models;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Copies the database that is about to change into a sibling schema named
/// <c>{database}_backup</c>, so a rebuild or a failed apply can be undone.
/// </summary>
/// <remarks>
/// Only tables and their rows are copied. A backup exists to get records back, and every view,
/// procedure, trigger, and seed is rebuilt from the SQL tree in source control anyway. Foreign keys
/// are deliberately absent from the copy (<c>CREATE TABLE ... LIKE</c> does not carry them), which
/// means tables can be filled in any order without disabling the checks.
/// </remarks>
public sealed class Service_DatabaseBackup
{
    public const string BackupSuffix = "_backup";

    private const int MaxIdentifierLength = 64;

    private readonly Service_MySqlGateway _gateway;

    public Service_DatabaseBackup(Service_MySqlGateway gateway)
    {
        _gateway = gateway;
    }

    /// <summary>
    /// Name of the schema this service writes the backup into.
    /// </summary>
    public static string BackupNameFor(string database) =>
        $"{database?.Trim()}{BackupSuffix}";

    /// <summary>
    /// Replaces <c>{database}_backup</c> with a copy of <paramref name="database"/>.
    /// </summary>
    /// <remarks>
    /// A database that does not exist yet has nothing to lose, so no backup schema is created and
    /// the call succeeds. Every other failure throws: the caller must not write to a database it
    /// could not back up.
    /// </remarks>
    public async Task<Model_DeploymentResult> BackupAsync(
        string database,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var result = new Model_DeploymentResult();
        var source = database?.Trim() ?? string.Empty;

        if (source.Length == 0)
        {
            throw new InvalidOperationException("No database was chosen, so nothing could be backed up.");
        }

        var backup = BackupNameFor(source);
        if (backup.Length > MaxIdentifierLength)
        {
            throw new InvalidOperationException(
                $"'{backup}' is longer than the {MaxIdentifierLength} characters MySQL allows for a name. "
                + "Shorten the database name or take a backup first.");
        }

        if (!await SchemaExistsAsync(source, cancellationToken).ConfigureAwait(false))
        {
            progress?.Report($"{source} does not exist yet, so there is nothing to back up.");
            result.AddMessage($"{source} did not exist yet. No backup was needed.");
            return result;
        }

        progress?.Report($"Backing up {source} to {backup}...");

        await _gateway.ExecuteWithoutDatabaseAsync(
            $"DROP DATABASE IF EXISTS {Quote(backup)};",
            cancellationToken).ConfigureAwait(false);
        await _gateway.ExecuteWithoutDatabaseAsync(
            $"CREATE DATABASE {Quote(backup)} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
            cancellationToken).ConfigureAwait(false);

        result.AddMessage($"Backup schema {backup} was recreated.");

        var tables = await ReadTablesAsync(source, cancellationToken).ConfigureAwait(false);
        foreach (var (table, columns) in tables)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"Backing up table {table}...");

            await _gateway.ExecuteAsync(
                source,
                $"CREATE TABLE {Quote(backup)}.{Quote(table)} LIKE {Quote(source)}.{Quote(table)};",
                cancellationToken).ConfigureAwait(false);

            // Generated columns cannot be inserted into, so the explicit list leaves them out and
            // lets MySQL compute them in the copy.
            if (columns.Count > 0)
            {
                var columnList = string.Join(", ", columns.Select(Quote));
                await _gateway.ExecuteAsync(
                    source,
                    $"INSERT INTO {Quote(backup)}.{Quote(table)} ({columnList}) "
                    + $"SELECT {columnList} FROM {Quote(source)}.{Quote(table)};",
                    cancellationToken).ConfigureAwait(false);
            }
        }

        var message = $"{tables.Count} table(s) copied from {source} into {backup}.";
        result.MarkApplied(message);
        progress?.Report(message);
        return result;
    }

    private async Task<bool> SchemaExistsAsync(string database, CancellationToken cancellationToken)
    {
        var rows = await _gateway.QueryAsync(
            "information_schema",
            "SELECT SCHEMA_NAME FROM SCHEMATA "
            + $"WHERE SCHEMA_NAME = '{database.Replace("'", "''", StringComparison.Ordinal)}';",
            cancellationToken).ConfigureAwait(false);

        return rows.Count > 0;
    }

    private async Task<List<(string Table, List<string> Columns)>> ReadTablesAsync(
        string database,
        CancellationToken cancellationToken)
    {
        var schema = database.Replace("'", "''", StringComparison.Ordinal);

        var tableRows = await _gateway.QueryAsync(
            "information_schema",
            "SELECT TABLE_NAME FROM TABLES "
            + $"WHERE TABLE_SCHEMA = '{schema}' AND TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;",
            cancellationToken).ConfigureAwait(false);

        var columnRows = await _gateway.QueryAsync(
            "information_schema",
            "SELECT TABLE_NAME, COLUMN_NAME FROM COLUMNS "
            + $"WHERE TABLE_SCHEMA = '{schema}' "
            + "AND EXTRA NOT LIKE '%VIRTUAL GENERATED%' AND EXTRA NOT LIKE '%STORED GENERATED%' "
            + "ORDER BY TABLE_NAME, ORDINAL_POSITION;",
            cancellationToken).ConfigureAwait(false);

        var columnsByTable = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in columnRows)
        {
            if (!columnsByTable.TryGetValue(row[0], out var list))
            {
                list = new List<string>();
                columnsByTable[row[0]] = list;
            }

            list.Add(row[1]);
        }

        return tableRows
            .Select(row => (
                row[0],
                columnsByTable.TryGetValue(row[0], out var columns) ? columns : new List<string>()))
            .ToList();
    }

    private static string Quote(string identifier) =>
        "`" + identifier.Replace("`", "``", StringComparison.Ordinal) + "`";
}
