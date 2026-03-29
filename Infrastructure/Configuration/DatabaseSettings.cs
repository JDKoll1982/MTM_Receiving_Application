namespace MTM_Receiving_Application.Infrastructure.Configuration;

/// <summary>
/// Database connection string configuration settings.
/// Populated from appsettings.json ConnectionStrings section.
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the MySQL database connection string for application data.
    /// </summary>
    public string MySqlConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Infor Visual database connection string for ERP integration.
    /// </summary>
    public string InforVisualConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command timeout in seconds applied to all stored procedure calls.
    /// Default: 30 seconds. Override in appsettings.json via ConnectionStrings:CommandTimeoutSeconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}
