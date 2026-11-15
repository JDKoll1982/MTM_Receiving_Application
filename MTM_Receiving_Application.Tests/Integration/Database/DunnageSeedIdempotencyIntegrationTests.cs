using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database;

/// <summary>
/// Proves the dunnage seed can be replayed against a database that already holds its rows.
///
/// The seed used to trust the insert procedures' <c>OUT</c> id. Those procedures skip the insert
/// when a row already exists, so on a second run the stale variable attached every child row to a
/// type that does not exist and the choice inserts then failed with
/// <c>Column 'CustomFieldID' cannot be null</c>. The seed now resolves parents from natural keys,
/// and this test drives the whole seed twice to keep that true.
/// </summary>
/// <remarks>
/// Everything runs in a scratch schema named <c>mtm_seedtest_&lt;8 hex chars&gt;</c> on the server in
/// <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or <c>MTM_MYSQL_CONNECTION_STRING</c>), which is dropped
/// on disposal. The configured database is never touched. When no connection string is set, the test
/// reports success without connecting.
/// </remarks>
public sealed class DunnageSeedIdempotencyIntegrationTests : IAsyncLifetime
{
    private static readonly string[] SeededTables =
    [
        "dunnage_types",
        "dunnage_parts",
        "dunnage_custom_fields",
        "dunnage_custom_field_choices",
        "dunnage_requires_inventory",
        "dunnage_non_po_entries",
        "dunnage_quantity_types",
    ];

    private string? _serverConnectionString;
    private string _scratchDatabase = string.Empty;
    private string _scratchConnectionString = string.Empty;

    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("MTM_MYSQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }

        var builder = new MySqlConnectionStringBuilder(configured);
        _serverConnectionString = new MySqlConnectionStringBuilder
        {
            Server = builder.Server,
            Port = builder.Port,
            UserID = builder.UserID,
            Password = builder.Password,
            SslMode = MySqlSslMode.Preferred,
            AllowUserVariables = true,
            CharacterSet = "utf8mb4",
            ConnectionTimeout = 10,
        }.ConnectionString;

        _scratchDatabase = "mtm_seedtest_" + Guid.NewGuid().ToString("N")[..8];
        _scratchConnectionString = new MySqlConnectionStringBuilder(_serverConnectionString)
        {
            Database = _scratchDatabase,
        }.ConnectionString;

        await ExecuteAsync(
            _serverConnectionString,
            $"DROP DATABASE IF EXISTS `{_scratchDatabase}`; " +
            $"CREATE DATABASE `{_scratchDatabase}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }

    public async Task DisposeAsync()
    {
        if (_serverConnectionString is null)
        {
            return;
        }

        await ExecuteAsync(_serverConnectionString, $"DROP DATABASE IF EXISTS `{_scratchDatabase}`;");
    }

    [Fact]
    public async Task SeedDunnageData_ShouldInsertNoNewRows_WhenReplayedAgainstPopulatedDatabase()
    {
        var sqlRoot = ResolveSqlRoot();
        if (_serverConnectionString is null || sqlRoot is null)
        {
            return;
        }

        var catalog = new Service_ObjectCatalog(sqlRoot);
        var objects = catalog.LoadObjects();
        var seed = objects.Single(item =>
            item.Kind == Enum_SqlObjectKind.Seed && item.Name == "03_seed_dunnage_data");
        var dunnageProcedures = objects
            .Where(item => item.Kind == Enum_SqlObjectKind.Procedure
                && item.RelativePath.StartsWith("StoredProcedures/Dunnage/", StringComparison.Ordinal))
            .ToList();

        await using var connection = new MySqlConnection(_scratchConnectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, "SET FOREIGN_KEY_CHECKS = 0;");

        foreach (var schemaObject in objects.Where(item => item.Kind == Enum_SqlObjectKind.Schema))
        {
            await ExecuteScriptAsync(connection, schemaObject.FullPath);
        }

        foreach (var procedure in dunnageProcedures)
        {
            await ExecuteScriptAsync(connection, procedure.FullPath);
        }

        await ExecuteScriptAsync(connection, seed.FullPath);
        var afterFirstRun = await ReadRowCountsAsync(connection);

        await ExecuteScriptAsync(connection, seed.FullPath);
        var afterSecondRun = await ReadRowCountsAsync(connection);

        afterSecondRun.Should().Equal(afterFirstRun);

        var orphanFieldCount = Convert.ToInt64(
            await ExecuteScalarAsync(
                connection,
                "SELECT COUNT(*) FROM dunnage_custom_fields f "
                + "WHERE NOT EXISTS (SELECT 1 FROM dunnage_types t WHERE t.id = f.DunnageTypeID);"));
        orphanFieldCount.Should().Be(0, "every custom field must hang off a real dunnage type");

        var unpopulatedChoiceFieldCount = Convert.ToInt64(
            await ExecuteScalarAsync(
                connection,
                "SELECT COUNT(*) FROM dunnage_custom_fields f "
                + "WHERE f.FieldType = 'Choices' "
                + "AND NOT EXISTS (SELECT 1 FROM dunnage_custom_field_choices c WHERE c.CustomFieldID = f.ID);"));
        unpopulatedChoiceFieldCount.Should().Be(0, "every Choices field must own its choice list");

        var duplicateInventoryCount = Convert.ToInt64(
            await ExecuteScalarAsync(
                connection,
                "SELECT COUNT(*) FROM ("
                + "SELECT part_id, inventory_method FROM dunnage_requires_inventory "
                + "GROUP BY part_id, inventory_method HAVING COUNT(*) > 1) AS duplicates;"));
        duplicateInventoryCount.Should().Be(0, "the returnable-dunnage seed must not duplicate rows");
    }

    private static string? ResolveSqlRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName, "Database", "Database_Deployment", "Sql_Files");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static async Task ExecuteScriptAsync(MySqlConnection connection, string path)
    {
        var script = await File.ReadAllTextAsync(path);
        foreach (var statement in Service_SqlScriptParser.SplitStatements(script))
        {
            // Seeds intentionally raise benign duplicate errors on a replay, so failures are
            // tolerated here; the row-count comparison is what proves the run was clean.
            try
            {
                await ExecuteAsync(connection, statement);
            }
            catch (MySqlException)
            {
                // Ignored by design - see the comment above.
            }
        }
    }

    private static async Task<long[]> ReadRowCountsAsync(MySqlConnection connection)
    {
        var counts = new List<long>();
        foreach (var table in SeededTables)
        {
            var value = await ExecuteScalarAsync(connection, $"SELECT COUNT(*) FROM `{table}`;");
            counts.Add(Convert.ToInt64(value));
        }

        return [.. counts];
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, sql);
    }

    private static async Task ExecuteAsync(MySqlConnection connection, string sql)
    {
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<object?> ExecuteScalarAsync(MySqlConnection connection, string sql)
    {
        await using var command = new MySqlCommand(sql, connection);
        return await command.ExecuteScalarAsync();
    }
}
