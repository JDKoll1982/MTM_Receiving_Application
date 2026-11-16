using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database;

/// <summary>
/// Proves a safe update leaves seed data alone once a database has taken it.
///
/// The engine used to replay every seed on every run. A run that rebuilt a schema and then applied
/// a safe update wrote the reference data twice. The ledger now records each seed's text against the
/// database, and this test drives a rebuild followed by a safe update to check the second run seeds
/// nothing.
/// </summary>
/// <remarks>
/// Everything runs in a scratch schema named <c>mtm_seedledgertest_&lt;8 hex chars&gt;</c> on the server in
/// <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or <c>MTM_MYSQL_CONNECTION_STRING</c>); the schema and the
/// backup the safe update takes are dropped afterwards. When no connection string is set, the test
/// reports success without connecting. The shipped SQL tree is deployed from the repository.
/// </remarks>
public sealed class Service_DeploymentEngineSeedLedgerTests
{
    [Fact]
    public async Task ApplyAsync_ShouldLeaveEverySeedAlone_WhenARebuildAlreadySeededTheDatabase()
    {
        var configured = Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("MTM_MYSQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }

        var sqlRoot = ResolveSqlRoot();
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

        var database = "mtm_seedledgertest_" + Guid.NewGuid().ToString("N")[..8];
        var backup = Service_DatabaseBackup.BackupNameFor(database);
        var catalog = new Service_ObjectCatalog(sqlRoot);
        var seedCount = catalog.LoadObjects().Count(item => item.Kind == Enum_SqlObjectKind.Seed);
        seedCount.Should().BePositive("the SQL tree ships seed scripts, otherwise this test proves nothing");

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

        var ledger = new Service_SeedLedger(settings);

        try
        {
            await ExecuteAsync(
                serverConnectionString,
                $"DROP DATABASE IF EXISTS `{database}`; "
                + $"CREATE DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");

            var gateway = new Service_MySqlGateway(settings);
            var engine = new Service_DeploymentEngine(gateway, catalog, ledger);

            var rebuilt = await engine.RebuildAsync(database, progress: null, CancellationToken.None);
            rebuilt.IsSuccess.Should().BeTrue();

            var plan = new Model_DeploymentPlan
            {
                Server = builder.Server,
                Port = (int)builder.Port,
                Database = database,
                GeneratedAt = DateTimeOffset.Now,
            };

            var applied = await engine.ApplyAsync(
                plan,
                dropConfirmedExtras: false,
                progress: null,
                CancellationToken.None);

            applied.IsSuccess.Should().BeTrue();
            applied.AlreadySeededCount.Should().Be(
                seedCount,
                "every seed already ran during the rebuild of this database");
            applied.Messages.Should().NotContain(
                message => message.StartsWith("Seeding:", StringComparison.Ordinal),
                "a safe update must not write reference data a second time");
            applied.AlreadySeeded.Should().HaveCount(seedCount);
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

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 0 };
        await command.ExecuteNonQueryAsync();
    }
}
