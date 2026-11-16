using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Deploys the whole SQL object tree into a throwaway schema so every stored procedure can be
/// exercised against the shape the repository actually ships.
/// </summary>
/// <remarks>
/// The schema is named <c>mtm_sptest_&lt;8 hex chars&gt;</c>, is created on the server resolved by
/// <see cref="StoredProcedureRepository.ResolveServer"/>, and is dropped on disposal. No existing
/// database is read or written, which is why the coverage tests can insert and delete freely.
/// </remarks>
public sealed class StoredProcedureTestDatabase : IAsyncLifetime
{
    private readonly List<string> _deploymentFailures = [];
    private string? _serverConnectionString;

    public string ScratchDatabase { get; } = "mtm_sptest_" + Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Connection string for the scratch schema, or <c>null</c> when no server is available.
    /// </summary>
    public string? ConnectionString { get; private set; }

    /// <summary>
    /// Why the fixture is not usable, or <c>null</c> when it is.
    /// </summary>
    public string? SkipReason { get; private set; }

    /// <summary>
    /// Scripts that failed to deploy. Empty on a healthy tree.
    /// </summary>
    public IReadOnlyList<string> DeploymentFailures => _deploymentFailures;

    /// <summary>
    /// Procedure scripts the fixture attempted to deploy.
    /// </summary>
    public IReadOnlyList<Model_SqlObject> ProcedureScripts { get; private set; } = [];

    public async Task InitializeAsync()
    {
        var sqlRoot = StoredProcedureRepository.ResolveSqlRoot();
        if (sqlRoot is null)
        {
            SkipReason = "The SQL object tree could not be located from the test output folder.";
            return;
        }

        var resolved = StoredProcedureRepository.ResolveServer();
        if (!resolved.IsAvailable)
        {
            SkipReason = resolved.Explanation;
            return;
        }

        _serverConnectionString = resolved.ConnectionString!;
        ConnectionString = new MySqlConnectionStringBuilder(_serverConnectionString)
        {
            Database = ScratchDatabase,
        }.ConnectionString;

        try
        {
            await ExecuteAsync(
                _serverConnectionString,
                $"DROP DATABASE IF EXISTS `{ScratchDatabase}`; "
                + $"CREATE DATABASE `{ScratchDatabase}` "
                + "CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

            await using var probe = new MySqlConnection(ConnectionString);
            await probe.OpenAsync();
        }
        catch (MySqlException exception)
        {
            // A server that is reachable but rejects the credentials must not turn the whole suite
            // red on a machine that never opted in to database tests.
            SkipReason =
                "The stored procedure coverage fixture could not use MySQL: "
                + $"{exception.Message} Set MTM_TEST_MYSQL_CONNECTION_STRING (or "
                + "MTM_MYSQL_CONNECTION_STRING) to an account that may create databases.";
            ConnectionString = null;
            return;
        }

        var objects = new Service_ObjectCatalog(sqlRoot).LoadObjects();
        ProcedureScripts = objects
            .Where(item => item.Kind == Enum_SqlObjectKind.Procedure)
            .ToList();

        foreach (var schemaObject in objects.Where(item => item.Kind == Enum_SqlObjectKind.Schema))
        {
            // Several table scripts turn foreign key checks back on in their last statement, so the
            // flag is re-asserted per statement instead of once per connection.
            await DeployAsync(schemaObject, forceForeignKeyChecksOff: true);
        }

        foreach (var kind in new[]
        {
            Enum_SqlObjectKind.View,
            Enum_SqlObjectKind.Procedure,
            Enum_SqlObjectKind.Trigger,
        })
        {
            foreach (var sqlObject in objects.Where(item => item.Kind == kind))
            {
                await DeployAsync(sqlObject, forceForeignKeyChecksOff: false);
            }
        }

        // Seed scripts carry reference data the read procedures expect. A seed that trips over its
        // own duplicate guard is not a coverage failure, so seed errors stay out of the report.
        foreach (var seed in objects.Where(item => item.Kind == Enum_SqlObjectKind.Seed))
        {
            try
            {
                await ExecuteScriptAsync(seed.FullPath);
            }
            catch (MySqlException)
            {
                // Reference data only.
            }
        }
    }

    public async Task DisposeAsync()
    {
        if (_serverConnectionString is null)
        {
            return;
        }

        try
        {
            await ExecuteAsync(
                _serverConnectionString,
                $"DROP DATABASE IF EXISTS `{ScratchDatabase}`;");
        }
        catch (MySqlException)
        {
            // A leftover scratch schema is harmless; failing disposal would hide the real result.
        }
    }

    /// <summary>
    /// Names of every procedure the database actually holds.
    /// </summary>
    public async Task<IReadOnlyList<string>> ReadDeployedProcedureNamesAsync()
    {
        if (ConnectionString is null)
        {
            return [];
        }

        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT routine_name FROM information_schema.routines "
            + "WHERE routine_schema = @schema AND routine_type = 'PROCEDURE' "
            + "ORDER BY routine_name;",
            connection);
        command.Parameters.AddWithValue("@schema", ScratchDatabase);

        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    /// <summary>
    /// Deploys one script on its own connection, which is what the deployment engine does. A script
    /// that changes the session - two scripts here run <c>SET NAMES ... COLLATE utf8mb4_general_ci</c> -
    /// must not be able to change how the next one is created.
    /// </summary>
    private async Task DeployAsync(Model_SqlObject sqlObject, bool forceForeignKeyChecksOff)
    {
        try
        {
            await ExecuteScriptAsync(sqlObject.FullPath, forceForeignKeyChecksOff);
        }
        catch (MySqlException exception)
        {
            _deploymentFailures.Add($"{sqlObject.RelativePath}: {exception.Message}");
        }
    }

    private async Task ExecuteScriptAsync(string path, bool forceForeignKeyChecksOff = false)
    {
        await using var connection = new MySqlConnection(ConnectionString!);
        await connection.OpenAsync();
        // Procedure bodies capture the session collation for their literals. The tables are
        // utf8mb4_unicode_ci, so a procedure created under another collation would fail at runtime
        // whenever it compares one of its own literals with a column.
        await ExecuteAsync(connection, "SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;");

        var script = await File.ReadAllTextAsync(path);
        foreach (var statement in Service_SqlScriptParser.SplitStatements(script))
        {
            if (forceForeignKeyChecksOff)
            {
                await ExecuteAsync(connection, "SET FOREIGN_KEY_CHECKS = 0;");
            }

            await ExecuteAsync(connection, statement);
        }
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, sql);
    }

    private static async Task ExecuteAsync(MySqlConnection connection, string sql)
    {
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync();
    }
}
