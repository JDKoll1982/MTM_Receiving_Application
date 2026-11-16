namespace MTM_Database_Deployment.Models;

/// <summary>
/// Connection settings for the deployment target. Values are loaded from deployment.settings.json
/// and never contain secrets that are committed to source control.
/// </summary>
public sealed class Model_ConnectionSettings
{
    public const string LiveDatabase = "mtm_receiving_application";
    public const string TestDatabase = "mtm_receiving_application_test";

    public string PreferredServer { get; set; } = "172.16.1.104";
    public string FallbackServer { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = LiveDatabase;
    public string User { get; set; } = "root";
    public string Password { get; set; } = "root";
    public int ConnectTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Databases the operator may pick from. A site can add its own names here without a rebuild.
    /// </summary>
    public List<string> AvailableDatabases { get; set; } = [LiveDatabase, TestDatabase];

    /// <summary>
    /// The selectable databases, guaranteed to contain <see cref="Database"/> so a name that is not in
    /// the list is never silently swapped for another one.
    /// </summary>
    public IReadOnlyList<string> ResolveAvailableDatabases()
    {
        var names = new List<string>();
        foreach (var candidate in AvailableDatabases ?? [])
        {
            var name = candidate?.Trim();
            if (!string.IsNullOrWhiteSpace(name)
                && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                names.Add(name);
            }
        }

        var current = Database?.Trim();
        if (!string.IsNullOrWhiteSpace(current)
            && !names.Contains(current, StringComparer.OrdinalIgnoreCase))
        {
            names.Insert(0, current);
        }

        if (names.Count == 0)
        {
            names.Add(LiveDatabase);
        }

        return names;
    }

    public Model_ConnectionSettings Clone() => new()
    {
        PreferredServer = PreferredServer,
        FallbackServer = FallbackServer,
        Port = Port,
        Database = Database,
        User = User,
        Password = Password,
        ConnectTimeoutSeconds = ConnectTimeoutSeconds,
        AvailableDatabases = [.. AvailableDatabases ?? []]
    };
}
