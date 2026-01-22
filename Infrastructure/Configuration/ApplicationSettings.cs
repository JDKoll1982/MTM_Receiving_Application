namespace MTM_Receiving_Application.Infrastructure.Configuration;

/// <summary>
/// General application configuration settings.
/// Populated from appsettings.json Application section.
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Gets or sets the application name displayed in UI and logs.
    /// </summary>
    public string ApplicationName { get; set; } = "MTM Receiving Application";

    /// <summary>
    /// Gets or sets the current environment (Development, Staging, Production).
    /// </summary>
    public string Environment { get; set; } = "Production";

    /// <summary>
    /// Gets or sets the application version string.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets whether to enable telemetry collection.
    /// </summary>
    public bool EnableTelemetry { get; set; } = false;
}
