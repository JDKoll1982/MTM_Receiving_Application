namespace MTM_Receiving_Application.Helpers.Database;

/// <summary>
/// Manages database connection strings for different environments
/// </summary>
public static class Helper_Database_Variables
{
    /// <summary>
    /// Production MySQL connection string
    /// Server: 172.16.1.104, Port: 3306, Database: mtm_receiving_application
    /// </summary>
    public static string ProductionConnectionString { get; } = 
        "Server=172.16.1.104;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";

    /// <summary>
    /// Test MySQL connection string (same as production for now)
    /// </summary>
    public static string TestConnectionString { get; } = 
        "Server=172.16.1.104;Port=3306;Database=mtm_receiving_application_test;Uid=root;Pwd=root;";

    /// <summary>
    /// Gets the connection string based on current environment
    /// </summary>
    /// <param name="useProduction">True for production, false for test</param>
    /// <returns>MySQL connection string</returns>
    public static string GetConnectionString(bool useProduction = true)
    {
        return useProduction ? ProductionConnectionString : TestConnectionString;
    }
}
