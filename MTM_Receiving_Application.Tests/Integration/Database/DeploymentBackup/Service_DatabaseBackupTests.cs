using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.DeploymentBackup;

/// <summary>
/// Proves the deployment tool keeps a usable copy of the database it is about to change.
///
/// The backup is the only thing standing between a confirmed rebuild and permanently lost records,
/// so the important behaviours are that every table and row lands in <c>{database}_backup</c>, that
/// generated columns do not break the copy, and that a stale backup from an earlier run is replaced
/// rather than kept.
/// </summary>
/// <remarks>
/// Everything runs in a scratch schema named <c>mtm_backuptest_&lt;8 hex chars&gt;</c> on the server in
/// <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or <c>MTM_MYSQL_CONNECTION_STRING</c>). Both the scratch
/// schema and its backup are dropped on disposal, and the configured database is never touched. When
/// no connection string is set, the tests report success without connecting.
/// </remarks>
public sealed class Service_DatabaseBackupTests : IAsyncLifetime
{
    private string? _serverConnectionString;
    private Model_ConnectionSettings? _settings;

    public string SourceDatabase { get; } = "mtm_backuptest_" + Guid.NewGuid().ToString("N")[..8];

    public string BackupDatabase => Service_DatabaseBackup.BackupNameFor(SourceDatabase);

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

        _settings = new Model_ConnectionSettings
        {
            PreferredServer = builder.Server,
            FallbackServer = builder.Server,
            Port = (int)builder.Port,
            Database = SourceDatabase,
            User = builder.UserID,
            Password = builder.Password,
            ConnectTimeoutSeconds = 10,
        };

        try
        {
            await ExecuteAsync(
                $"DROP DATABASE IF EXISTS `{SourceDatabase}`; "
                + $"CREATE DATABASE `{SourceDatabase}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
        }
        catch (MySqlException)
        {
            // A reachable server that refuses the account must not turn the suite red.
            _settings = null;
            return;
        }

        await ExecuteAsync(
            "CREATE TABLE `widgets` ("
            + "`id` INT NOT NULL, "
            + "`label` VARCHAR(32) NOT NULL, "
            + "`label_upper` VARCHAR(32) GENERATED ALWAYS AS (UPPER(`label`)) STORED, "
            + "PRIMARY KEY (`id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;",
            SourceDatabase);
        await ExecuteAsync(
            "INSERT INTO `widgets` (`id`, `label`) VALUES (1, 'first'), (2, 'second');",
            SourceDatabase);
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
                $"DROP DATABASE IF EXISTS `{SourceDatabase}`; "
                + $"DROP DATABASE IF EXISTS `{BackupDatabase}`;");
        }
        catch (MySqlException)
        {
            // A leftover scratch schema is harmless; failing disposal would hide the real result.
        }
    }

    [Fact]
    public async Task BackupAsync_ShouldCopyEveryTableAndRow_WhenSourceHasData()
    {
        if (_settings is null)
        {
            return;
        }

        var service = new Service_DatabaseBackup(new Service_MySqlGateway(_settings));

        var result = await service.BackupAsync(SourceDatabase, progress: null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Messages.Should().Contain(message => message.Contains("1 table(s) copied"));

        var rows = await QueryAsync(
            BackupDatabase,
            "SELECT `id`, `label`, `label_upper` FROM `widgets` ORDER BY `id`;");

        rows.Should().HaveCount(2);
        rows[0].Should().Equal("1", "first", "FIRST");
        rows[1].Should().Equal("2", "second", "SECOND");
    }

    [Fact]
    public async Task BackupAsync_ShouldReplaceTheEarlierBackup_WhenOneAlreadyExists()
    {
        if (_settings is null)
        {
            return;
        }

        await ExecuteAsync(
            $"DROP DATABASE IF EXISTS `{BackupDatabase}`; "
            + $"CREATE DATABASE `{BackupDatabase}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; "
            + $"CREATE TABLE `{BackupDatabase}`.`widgets` (`id` INT NOT NULL, `label` VARCHAR(32) NOT NULL); "
            + $"INSERT INTO `{BackupDatabase}`.`widgets` (`id`, `label`) VALUES (99, 'stale');");

        var service = new Service_DatabaseBackup(new Service_MySqlGateway(_settings));

        await service.BackupAsync(SourceDatabase, progress: null, CancellationToken.None);

        var rows = await QueryAsync(
            BackupDatabase,
            "SELECT `id`, `label` FROM `widgets` ORDER BY `id`;");

        rows.Should().HaveCount(2);
        rows.Should().NotContain(row => row[1] == "stale");
    }

    [Fact]
    public async Task BackupAsync_ShouldSucceedWithoutCreatingABackup_WhenSourceDoesNotExist()
    {
        if (_settings is null)
        {
            return;
        }

        var missing = "mtm_backuptest_missing_" + Guid.NewGuid().ToString("N")[..8];
        var service = new Service_DatabaseBackup(new Service_MySqlGateway(_settings));

        var result = await service.BackupAsync(missing, progress: null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.AppliedCount.Should().Be(0);

        var schemas = await QueryAsync(
            database: null,
            "SELECT SCHEMA_NAME FROM information_schema.SCHEMATA "
            + $"WHERE SCHEMA_NAME = '{Service_DatabaseBackup.BackupNameFor(missing)}';");

        schemas.Should().BeEmpty();
    }

    private async Task ExecuteAsync(string sql, string? database = null)
    {
        await using var connection = new MySqlConnection(WithDatabase(database));
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync();
    }

    private async Task<List<string[]>> QueryAsync(string? database, string sql)
    {
        var rows = new List<string[]>();
        await using var connection = new MySqlConnection(WithDatabase(database));
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 120 };
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var values = new string[reader.FieldCount];
            for (var field = 0; field < reader.FieldCount; field++)
            {
                values[field] = reader.IsDBNull(field)
                    ? string.Empty
                    : Convert.ToString(reader.GetValue(field)) ?? string.Empty;
            }

            rows.Add(values);
        }

        return rows;
    }

    private string WithDatabase(string? database) =>
        new MySqlConnectionStringBuilder(_serverConnectionString!)
        {
            Database = database ?? string.Empty,
        }.ConnectionString;
}
