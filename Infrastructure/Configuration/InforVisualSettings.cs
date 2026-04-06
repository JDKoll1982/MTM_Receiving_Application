namespace MTM_Receiving_Application.Infrastructure.Configuration;

/// <summary>
/// Infor Visual ERP integration configuration settings.
/// Populated from appsettings.json InforVisual section.
/// </summary>
public class InforVisualSettings
{
    /// <summary>
    /// Gets or sets the connection timeout in seconds for Infor Visual database queries.
    /// Default: 30 seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the query timeout in seconds for long-running Infor Visual operations.
    /// Default: 120 seconds.
    /// </summary>
    public int QueryTimeoutSeconds { get; set; } = 120;
}
