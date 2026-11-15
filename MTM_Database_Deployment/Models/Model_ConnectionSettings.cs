namespace MTM_Database_Deployment.Models;

/// <summary>
/// Connection settings for the deployment target. Values are loaded from deployment.settings.json
/// and never contain secrets that are committed to source control.
/// </summary>
public sealed class Model_ConnectionSettings
{
    public string PreferredServer { get; set; } = "172.16.1.104";
    public string FallbackServer { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = "mtm_receiving_application";
    public string User { get; set; } = "root";
    public string Password { get; set; } = "root";
    public int ConnectTimeoutSeconds { get; set; } = 5;

    public Model_ConnectionSettings Clone() => new()
    {
        PreferredServer = PreferredServer,
        FallbackServer = FallbackServer,
        Port = Port,
        Database = Database,
        User = User,
        Password = Password,
        ConnectTimeoutSeconds = ConnectTimeoutSeconds
    };
}
