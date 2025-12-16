namespace MTM_Receiving_Application.Helpers.Database;

/// <summary>
/// Manages database connection strings for different environments
/// </summary>
public static class Helper_Database_Variables
{
    /// <summary>
    /// Production MySQL connection string
    /// Server: localhost, Port: 3306, Database: mtm_receiving_db
    /// </summary>
    public static string ProductionConnectionString { get; } = 
        "Server=localhost;Port=3306;Database=mtm_receiving_db;Uid=root;Pwd=root;";

    /// <summary>
    /// Test MySQL connection string (same as production for now)
    /// </summary>
    public static string TestConnectionString { get; } = 
        "Server=localhost;Port=3306;Database=mtm_receiving_db_test;Uid=root;Pwd=root;";

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
