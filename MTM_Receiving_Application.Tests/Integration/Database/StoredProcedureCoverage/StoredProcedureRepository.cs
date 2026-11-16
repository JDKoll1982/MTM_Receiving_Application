using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Locates the shipped SQL object tree, the stored procedures it declares, and the MySQL server the
/// coverage tests may use.
/// </summary>
internal static partial class StoredProcedureRepository
{
    private static readonly string[] SqlRootSegments = ["Database", "Database_Deployment", "Sql_Files"];

    private static readonly string[] ConnectionStringVariables =
    [
        "MTM_TEST_MYSQL_CONNECTION_STRING",
        "MTM_MYSQL_CONNECTION_STRING",
    ];

    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Walks up from the test output folder to the folder that holds the solution and database tree.
    /// </summary>
    public static string? ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine([directory.FullName, .. SqlRootSegments])))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    public static string? ResolveSqlRoot()
    {
        var root = ResolveRepositoryRoot();
        return root is null ? null : Path.Combine([root, .. SqlRootSegments]);
    }

    public static IReadOnlyList<Model_SqlObject> LoadSqlObjects()
    {
        var sqlRoot = ResolveSqlRoot();
        return sqlRoot is null ? [] : new Service_ObjectCatalog(sqlRoot).LoadObjects();
    }

    /// <summary>
    /// Every procedure name the shipped tree declares, in ordinal order. A single script may hold
    /// several procedures, so the script count and the procedure count are not the same number.
    /// </summary>
    public static IReadOnlyList<string> LoadProcedureNames()
    {
        return LoadSqlObjects()
            .Where(item => item.Kind == Enum_SqlObjectKind.Procedure)
            .SelectMany(item => ReadDeclaredProcedureNames(item.FullPath))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Reads every procedure name a script declares. Empty when the script declares none.
    /// </summary>
    public static IReadOnlyList<string> ReadDeclaredProcedureNames(string scriptPath)
    {
        return CreateProcedurePattern()
            .Matches(File.ReadAllText(scriptPath))
            .Select(match => match.Groups["name"].Value)
            .ToList();
    }

    /// <summary>
    /// Test sources, used to prove that every shipped procedure is named by at least one test.
    /// </summary>
    public static IReadOnlyList<string> LoadTestSourceFiles()
    {
        var root = ResolveRepositoryRoot();
        if (root is null)
        {
            return [];
        }

        var testProject = Path.Combine(root, "MTM_Receiving_Application.Tests");
        if (!Directory.Exists(testProject))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(testProject, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path, testProject))
            .ToList();
    }

    private static bool IsBuildOutput(string path, string testProject)
    {
        var relative = Path.GetRelativePath(testProject, path);
        return relative
            .Split(Path.DirectorySeparatorChar)
            .Any(segment =>
                segment.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || segment.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolves a server for the coverage tests: a configured connection string first, then the
    /// deployment settings file using the same reachable-server probe the tool itself runs.
    /// </summary>
    public static ResolvedConnection ResolveServer()
    {
        foreach (var variable in ConnectionStringVariables)
        {
            var configured = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return ResolvedConnection.Available(
                    NormalizeServerConnectionString(configured),
                    variable);
            }
        }

        var settingsPath = FindSettingsFile();
        if (settingsPath is null)
        {
            return ResolvedConnection.Unavailable(
                "Set MTM_TEST_MYSQL_CONNECTION_STRING (or MTM_MYSQL_CONNECTION_STRING) to a MySQL "
                + "account that may create databases. No deployment.settings.json was found either.");
        }

        Model_ConnectionSettings settings;
        try
        {
            settings = JsonSerializer.Deserialize<Model_ConnectionSettings>(
                File.ReadAllText(settingsPath),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new Model_ConnectionSettings();
        }
        catch (JsonException exception)
        {
            return ResolvedConnection.Unavailable(
                $"{settingsPath} could not be read: {exception.Message}");
        }

        foreach (var server in new[] { settings.PreferredServer, settings.FallbackServer })
        {
            if (string.IsNullOrWhiteSpace(server) || !CanReach(server, settings.Port))
            {
                continue;
            }

            var builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                Port = (uint)settings.Port,
                UserID = settings.User,
                Password = settings.Password,
                ConnectionTimeout = 10,
            };

            return ResolvedConnection.Available(
                NormalizeServerConnectionString(builder.ConnectionString),
                $"{settingsPath} using {server}");
        }

        return ResolvedConnection.Unavailable(
            $"Neither {settings.PreferredServer} nor {settings.FallbackServer} answered on port "
            + $"{settings.Port}, and no connection string environment variable is set.");
    }

    /// <summary>
    /// Keeps the caller's credentials but drops the database, because the fixture creates its own
    /// throwaway schema.
    /// </summary>
    private static string NormalizeServerConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString)
        {
            AllowUserVariables = true,
            CharacterSet = "utf8mb4",
            SslMode = MySqlSslMode.Preferred,
        };

        if (builder.ConnectionTimeout < 10)
        {
            builder.ConnectionTimeout = 10;
        }

        return new MySqlConnectionStringBuilder(builder.ConnectionString)
        {
            Database = string.Empty,
        }.ConnectionString;
    }

    private static string? FindSettingsFile()
    {
        var root = ResolveRepositoryRoot();
        if (root is null)
        {
            return null;
        }

        var projectPath = Path.Combine(
            root, "MTM_Database_Deployment", "deployment.settings.json");
        if (File.Exists(projectPath))
        {
            return projectPath;
        }

        var outputPath = Path.Combine(
            root, "MTM_Database_Deployment", "bin", "x64", "Debug",
            "net10.0-windows10.0.22621.0", "deployment.settings.json");

        return File.Exists(outputPath) ? outputPath : null;
    }

    private static bool CanReach(string server, int port)
    {
        try
        {
            using var client = new TcpClient();
            return client.ConnectAsync(server, port).Wait(ProbeTimeout) && client.Connected;
        }
        catch (Exception exception) when (exception is SocketException or AggregateException)
        {
            return false;
        }
    }

    [GeneratedRegex(
        @"^[ \t]*CREATE[ \t]+(?:DEFINER[ \t]*=[ \t]*\S+[ \t]+)?PROCEDURE[ \t]+`?(?<name>\w+)`?",
        RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex CreateProcedurePattern();
}

/// <summary>
/// The outcome of resolving a MySQL server for the coverage tests.
/// </summary>
internal sealed record ResolvedConnection(string? ConnectionString, string Explanation)
{
    public bool IsAvailable => ConnectionString is not null;

    public static ResolvedConnection Available(string connectionString, string source) =>
        new(connectionString, $"MySQL server taken from {source}.");

    public static ResolvedConnection Unavailable(string reason) => new(null, reason);
}
