namespace MTM_Receiving_Application.Module_Core.Models.Systems;

/// <summary>
/// Application settings loaded from appsettings.json
/// </summary>
public class Model_AppSettings
{
    /// <summary>
    /// If true, uses mock data instead of querying Infor Visual database
    /// Useful for development/testing when Infor Visual server is unavailable
    /// </summary>
    public bool UseInforVisualMockData { get; set; } = true;

    /// <summary>
    /// Environment name (Development, Production, etc.)
    /// </summary>
    public string Environment { get; set; } = "Development";
}

