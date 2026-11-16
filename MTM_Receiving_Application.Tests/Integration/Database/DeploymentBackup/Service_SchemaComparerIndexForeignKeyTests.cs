using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.DeploymentBackup;

/// <summary>
/// Proves an index rebuild releases the foreign key that depends on it.
///
/// MySQL refuses <c>DROP INDEX</c> with "Cannot drop index '...': needed in a foreign key
/// constraint" while a key still relies on that index. Live databases that were created before the
/// current table script carry exactly that drift - <c>dunnage_custom_fields.IDX_CUSTOM_001</c> is a
/// plain key on <c>DunnageTypeID</c> instead of the unique <c>(DunnageTypeID, DisplayOrder)</c> the
/// script declares - so the plan must drop the key, swap the index, and put the key back.
/// </summary>
/// <remarks>
/// Runs in a scratch schema named <c>mtm_idxfk_&lt;8 hex chars&gt;</c> that reproduces only the two
/// tables involved, on the server in <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or
/// <c>MTM_MYSQL_CONNECTION_STRING</c>). The schema is dropped afterwards. When no connection string
/// is set, the test reports success without connecting.
/// </remarks>
public sealed class Service_SchemaComparerIndexForeignKeyTests
{
    [Fact]
    public async Task BuildPlanAsync_ShouldDropTheDependentForeignKey_WhenAnIndexItServesIsRebuilt()
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

        var database = "mtm_idxfk_" + Guid.NewGuid().ToString("N")[..8];

        try
        {
            await CreateDriftedSchemaAsync(serverConnectionString, database);

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

            var comparer = new Service_SchemaComparer(
                new Service_MySqlGateway(settings),
                new Service_ObjectCatalog(sqlRoot));

            var plan = await comparer.BuildPlanAsync(database, progress: null, CancellationToken.None);

            var indexChange = plan.Changes.Single(change =>
                change.Target == "dunnage_custom_fields.IDX_CUSTOM_001");

            indexChange.Kind.Should().Be(Enum_ChangeKind.AddIndex);
            indexChange.Detail.Should().Contain("dependent foreign key");

            indexChange.Sql.Should().NotBeNullOrWhiteSpace();
            var sql = indexChange.Sql!;

            // Two statements: MySQL checks ADD clauses before DROP clauses, so the constraint cannot
            // be dropped and re-added under the same name in one ALTER. The key release and the index
            // swap stay together, because the index may never vanish while a key still points at it.
            var statements = Service_SqlScriptParser.SplitStatements(sql);
            statements.Should().HaveCount(2);

            statements[0].Should().StartWith("ALTER TABLE `" + database + "`.`dunnage_custom_fields`");
            statements[0].Should().Contain("DROP FOREIGN KEY `dunnage_custom_fields_ibfk_1`");
            statements[0].Should().Contain("DROP INDEX `IDX_CUSTOM_001`");
            statements[0].Should().Contain("ADD UNIQUE INDEX `IDX_CUSTOM_001` (`DunnageTypeID`, `DisplayOrder`)");

            statements[0].IndexOf("DROP FOREIGN KEY", StringComparison.Ordinal)
                .Should().BeLessThan(statements[0].IndexOf("DROP INDEX", StringComparison.Ordinal),
                    "the key has to go before its index can");

            statements[1].Should().Contain("ADD CONSTRAINT `dunnage_custom_fields_ibfk_1`");

            // The foreign key change must not try to drop the key a second time.
            plan.Changes
                .Where(change => change.Target == "dunnage_custom_fields.dunnage_custom_fields_ibfk_1")
                .Should().BeEmpty();
        }
        finally
        {
            try
            {
                await ExecuteAsync(serverConnectionString, $"DROP DATABASE IF EXISTS `{database}`;");
            }
            catch (MySqlException)
            {
                // A leftover scratch schema is harmless; failing cleanup would hide the real result.
            }
        }
    }

    [Fact]
    public async Task ApplyAsync_ShouldAttachTheForeignKey_WhenExistingRowsHaveNoParent()
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

        var database = "mtm_idxfk_" + Guid.NewGuid().ToString("N")[..8];

        try
        {
            await CreateDriftedSchemaAsync(serverConnectionString, database);

            // Live databases here reach this state through seeds that run with foreign key checks
            // off, so a constraint cannot be validated against these rows on the way back in.
            await ExecuteAsync(
                serverConnectionString,
                $"SET FOREIGN_KEY_CHECKS = 0; "
                + $"INSERT INTO `{database}`.`dunnage_custom_fields` "
                + "(`DunnageTypeID`, `FieldName`, `FieldType`, `DisplayOrder`, `CreatedBy`) "
                + "VALUES (9999, 'ORPHAN', 'Text', 1, 'TEST');");

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

            var gateway = new Service_MySqlGateway(settings);
            var catalog = new Service_ObjectCatalog(sqlRoot);
            var plan = await new Service_SchemaComparer(gateway, catalog)
                .BuildPlanAsync(database, progress: null, CancellationToken.None);

            var result = await new Service_DeploymentEngine(gateway, catalog)
                .ApplyAsync(plan, dropConfirmedExtras: false, progress: null, CancellationToken.None);

            result.IsSuccess.Should().BeTrue(result.ToReportText());
            result.Messages.Should().Contain(message =>
                message.Contains("attached with foreign key checks off"));

            var foreignKeys = await QueryAsync(
                serverConnectionString,
                "SELECT CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE "
                + $"WHERE TABLE_SCHEMA = '{database}' AND TABLE_NAME = 'dunnage_custom_fields' "
                + "AND REFERENCED_TABLE_NAME IS NOT NULL;");
            foreignKeys.Should().Contain("dunnage_custom_fields_ibfk_1");

            var indexColumns = await QueryAsync(
                serverConnectionString,
                "SELECT CONCAT(IF(NON_UNIQUE = 0, 'UNIQUE ', 'INDEX '), COLUMN_NAME) "
                + "FROM information_schema.STATISTICS "
                + $"WHERE TABLE_SCHEMA = '{database}' AND TABLE_NAME = 'dunnage_custom_fields' "
                + "AND INDEX_NAME = 'IDX_CUSTOM_001' ORDER BY SEQ_IN_INDEX;");
            indexColumns.Should().Equal("UNIQUE DunnageTypeID", "UNIQUE DisplayOrder");
        }
        finally
        {
            try
            {
                await ExecuteAsync(serverConnectionString, $"DROP DATABASE IF EXISTS `{database}`;");
            }
            catch (MySqlException)
            {
                // A leftover scratch schema is harmless; failing cleanup would hide the real result.
            }
        }
    }

    /// <summary>
    /// Two tables that reproduce the drift a live database carries: <c>IDX_CUSTOM_001</c> is a plain
    /// key on <c>DunnageTypeID</c> instead of the unique <c>(DunnageTypeID, DisplayOrder)</c> key the
    /// table script declares, and that plain key is the one the foreign key leans on.
    /// </summary>
    private static Task CreateDriftedSchemaAsync(string connectionString, string database) =>
        ExecuteAsync(
            connectionString,
            $"DROP DATABASE IF EXISTS `{database}`; "
            + $"CREATE DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; "
            + $"CREATE TABLE `{database}`.`dunnage_types` ("
            + "`id` int NOT NULL AUTO_INCREMENT, `type_name` varchar(100) NOT NULL, PRIMARY KEY (`id`)"
            + ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci; "
            + $"CREATE TABLE `{database}`.`dunnage_custom_fields` ("
            + "`ID` int NOT NULL AUTO_INCREMENT, "
            + "`DunnageTypeID` int NOT NULL, "
            + "`FieldName` varchar(100) NOT NULL, "
            + "`FieldType` varchar(20) NOT NULL, "
            + "`DisplayOrder` int NOT NULL, "
            + "`IsRequired` tinyint(1) NOT NULL DEFAULT '0', "
            + "`Unit` varchar(50) DEFAULT NULL, "
            + "`MinValue` decimal(18,4) DEFAULT NULL, "
            + "`MaxValue` decimal(18,4) DEFAULT NULL, "
            + "`DefaultValue` varchar(255) DEFAULT NULL, "
            + "`ValidationRules` text, "
            + "`CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP, "
            + "`CreatedBy` varchar(50) NOT NULL, "
            + "PRIMARY KEY (`ID`), "
            + "UNIQUE KEY `IDX_CUSTOM_002` (`DunnageTypeID`,`FieldName`), "
            + "KEY `IDX_CUSTOM_001` (`DunnageTypeID`), "
            + "KEY `IDX_CUSTOM_003` (`DunnageTypeID`), "
            + "CONSTRAINT `dunnage_custom_fields_ibfk_1` FOREIGN KEY (`DunnageTypeID`) "
            + "REFERENCES `dunnage_types` (`id`) ON DELETE CASCADE"
            + ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;");

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

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 0 };
        await command.ExecuteNonQueryAsync();
    }
}
