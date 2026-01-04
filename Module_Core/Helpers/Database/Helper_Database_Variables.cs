namespace MTM_Receiving_Application.Module_Core.Helpers.Database;

/// <summary>
/// Manages database connection strings for different environments
/// </summary>
public static class Helper_Database_Variables
{
    /// <summary>
    /// Production MySQL connection string
    /// Server: localhost, Port: 3306, Database: mtm_receiving_application
    /// </summary>
    public static string ProductionConnectionString { get; } =
        "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;CharSet=utf8mb4;";

    /// <summary>
    /// Test MySQL connection string (same as production for now)
    /// </summary>
    public static string TestConnectionString { get; } =
        "Server=localhost;Port=3306;Database=mtm_receiving_application_test;Uid=root;Pwd=root;CharSet=utf8mb4;";

    /// <summary>
    /// Gets the connection string based on current environment
    /// </summary>
    /// <param name="useProduction">True for production, false for test</param>
    /// <returns>MySQL connection string</returns>
    public static string GetConnectionString(bool useProduction = true)
    {
        return useProduction ? ProductionConnectionString : TestConnectionString;
    }

    /// <summary>
    /// Gets the Infor Visual connection string (READ ONLY)
    /// </summary>
    public static string GetInforVisualConnectionString()
    {
        return "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;";
    }
}

