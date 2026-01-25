using System;
using System.IO;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Application-wide configuration and constants
/// </summary>
public class Model_Application_Variables
{
    /// <summary>
    /// Display name of the application
    /// </summary>
    public string ApplicationName { get; set; } = "MTM Receiving Label Application";

    /// <summary>
    /// Semantic version of the application (MAJOR.MINOR.PATCH)
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// MySQL connection string (configured from Helper_Database_Variables)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Directory path for application log files
    /// </summary>
    public string LogDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MTM_Receiving_Application",
        "Logs"
    );

    /// <summary>
    /// Current environment (Development, Test, Production)
    /// </summary>
    public string EnvironmentType { get; set; } = "Development";
}
