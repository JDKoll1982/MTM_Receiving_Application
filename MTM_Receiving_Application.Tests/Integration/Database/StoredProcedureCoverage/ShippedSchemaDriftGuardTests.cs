using System.Text.RegularExpressions;
using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Fails when the configured test database no longer matches the SQL object tree that ships.
/// </summary>
/// <remarks>
/// The rest of the integration suite assumes the database it connects to was deployed from
/// <c>Database/Database_Deployment/Sql_Files</c>. When that database is stale — for example a
/// schema from an older generation of the tree — hundreds of tests fail for reasons that say
/// nothing about the code under test. This guard turns that situation into one failure that names
/// the missing objects.
/// </remarks>
public sealed partial class ShippedSchemaDriftGuardTests
{
    [Fact]
    public async Task Database_ShouldContainEveryShippedObject_WhenTestDatabaseIsConfigured()
    {
        var connectionString = Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var database = new MySqlConnectionStringBuilder(connectionString).Database;
        database.Should().NotBeNullOrWhiteSpace(
            "MTM_TEST_MYSQL_CONNECTION_STRING must name the database to verify");

        var shippedTables = LoadShippedNames("Schemas", TablePattern());
        var shippedViews = LoadShippedNames("Views", ViewPattern());
        var shippedProcedures = StoredProcedureRepository.LoadProcedureNames();

        shippedTables.Should().NotBeEmpty("the shipped SQL tree must declare tables");
        shippedProcedures.Should().NotBeEmpty("the shipped SQL tree must declare procedures");

        var liveTables = await LoadLiveNamesAsync(connectionString, database!, "BASE TABLE");
        var liveViews = await LoadLiveNamesAsync(connectionString, database!, "VIEW");
        var liveProcedures = await LoadLiveRoutineNamesAsync(connectionString, database!);

        Describe("table", shippedTables, liveTables, database!).Should().BeNull();
        Describe("view", shippedViews, liveViews, database!).Should().BeNull();
        Describe("procedure", shippedProcedures, liveProcedures, database!).Should().BeNull();
    }

    private static string? Describe(
        string kind,
        IReadOnlyCollection<string> shipped,
        IReadOnlyCollection<string> live,
        string database)
    {
        var missing = shipped.Except(live, StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();
        if (missing.Count == 0)
        {
            return null;
        }

        var listed = string.Join(", ", missing.Take(25));
        var more = missing.Count > 25 ? $" and {missing.Count - 25} more" : string.Empty;

        missing
            .Should()
            .BeEmpty(
                $"`{database}` is missing {missing.Count} shipped {kind}(s): {listed}{more}. "
                    + "Deploy Database/Database_Deployment/Sql_Files with MTM_Database_Deployment "
                    + "before running the integration tests.");
        return listed;
    }

    private static IReadOnlyList<string> LoadShippedNames(string folder, Regex pattern)
    {
        var sqlRoot = StoredProcedureRepository.ResolveSqlRoot();
        if (sqlRoot is null)
        {
            return [];
        }

        var directory = Path.Combine(sqlRoot, folder);
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(directory, "*.sql", SearchOption.AllDirectories)
            .SelectMany(path => pattern.Matches(File.ReadAllText(path))
                .Select(match => match.Groups["name"].Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<IReadOnlyList<string>> LoadLiveNamesAsync(
        string connectionString,
        string database,
        string tableType)
    {
        var names = new List<string>();
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT TABLE_NAME FROM information_schema.TABLES "
                + "WHERE TABLE_SCHEMA = @database AND TABLE_TYPE = @tableType;",
            connection);
        command.Parameters.AddWithValue("@database", database);
        command.Parameters.AddWithValue("@tableType", tableType);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private static async Task<IReadOnlyList<string>> LoadLiveRoutineNamesAsync(
        string connectionString,
        string database)
    {
        var names = new List<string>();
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT ROUTINE_NAME FROM information_schema.ROUTINES "
                + "WHERE ROUTINE_SCHEMA = @database AND ROUTINE_TYPE = 'PROCEDURE';",
            connection);
        command.Parameters.AddWithValue("@database", database);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    [GeneratedRegex(
        @"^[ \t]*CREATE[ \t]+TABLE[ \t]+(?:IF[ \t]+NOT[ \t]+EXISTS[ \t]+)?`?(?<name>\w+)`?",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex TablePattern();

    [GeneratedRegex(
        @"^[ \t]*CREATE[ \t]+(?:OR[ \t]+REPLACE[ \t]+)?(?:ALGORITHM[ \t]*=[ \t]*\w+[ \t]+)?(?:DEFINER[ \t]*=[ \t]*\S+[ \t]+)?(?:SQL[ \t]+SECURITY[ \t]+\w+[ \t]+)?VIEW[ \t]+`?(?<name>\w+)`?",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ViewPattern();
}
