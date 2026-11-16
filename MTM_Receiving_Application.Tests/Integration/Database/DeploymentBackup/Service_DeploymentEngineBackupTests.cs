using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.DeploymentBackup;

/// <summary>
/// Proves a confirmed rebuild copies the database it is about to erase.
///
/// The order matters more than the copy: a rebuild drops the target, so a backup taken afterwards
/// would be empty. This test writes a marker table that the shipped SQL tree does not contain,
/// rebuilds the schema, and checks the marker survives in <c>{database}_backup</c> while the rebuilt
/// database no longer has it.
/// </summary>
/// <remarks>
/// Runs in a scratch schema named <c>mtm_rebuildtest_&lt;8 hex chars&gt;</c> on the server in
/// <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or <c>MTM_MYSQL_CONNECTION_STRING</c>); the schema and its
/// backup are dropped afterwards. When no connection string is set, the test reports success without
/// connecting. The shipped SQL tree is deployed from the repository, so this test only makes sense
/// next to it.
/// </remarks>
public sealed class Service_DeploymentEngineBackupTests
{
    [Fact]
    public async Task RebuildAsync_ShouldCopyTheOldRowsIntoTheBackupSchema_WhenTargetHoldsData()
    {
        var configured = Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("MTM_MYSQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }

        var sqlRoot = StoredProcedureRepository.ResolveSqlRoot();
        if (sqlRoot is null)
        {
            return;
        }

        var builder = new MySqlConnectionStringBuilder(configured);
        var serverConnectionString = new MySqlConnectionStringBuilder
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

        var database = "mtm_rebuildtest_" + Guid.NewGuid().ToString("N")[..8];
        var backup = Service_DatabaseBackup.BackupNameFor(database);

        try
        {
            await ExecuteAsync(
                serverConnectionString,
                $"DROP DATABASE IF EXISTS `{database}`; "
                + $"CREATE DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; "
                + $"CREATE TABLE `{database}`.`marker` (`id` INT NOT NULL, PRIMARY KEY (`id`)); "
                + $"INSERT INTO `{database}`.`marker` (`id`) VALUES (7);");

            var settings = new Model_ConnectionSettings
            {
                PreferredServer = builder.Server,
                FallbackServer = builder.Server,
                Port = (int)builder.Port,
                Database = database,
                User = builder.UserID,
                Password = builder.Password,
                ConnectTimeoutSeconds = 10,
            };

            var engine = new Service_DeploymentEngine(
                new Service_MySqlGateway(settings),
                new Service_ObjectCatalog(sqlRoot));

            var result = await engine.RebuildAsync(database, progress: null, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Messages.Should().Contain(message => message.Contains("Backup:"));

            var backedUp = await QueryAsync(
                serverConnectionString,
                $"SELECT `id` FROM `{backup}`.`marker`;");
            backedUp.Should().Equal("7");

            var rebuilt = await QueryAsync(
                serverConnectionString,
                "SELECT TABLE_NAME FROM information_schema.TABLES "
                + $"WHERE TABLE_SCHEMA = '{database}' AND TABLE_NAME = 'marker';");
            rebuilt.Should().BeEmpty();
        }
        finally
        {
            try
            {
                await ExecuteAsync(
                    serverConnectionString,
                    $"DROP DATABASE IF EXISTS `{database}`; DROP DATABASE IF EXISTS `{backup}`;");
            }
            catch (MySqlException)
            {
                // A leftover scratch schema is harmless; failing cleanup would hide the real result.
            }
        }
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 0 };
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<List<string>> QueryAsync(string connectionString, string sql)
    {
        var values = new List<string>();
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 0 };
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            values.Add(Convert.ToString(reader.GetValue(0)) ?? string.Empty);
        }

        return values;
    }
}
