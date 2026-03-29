using Microsoft.Extensions.Configuration;

namespace MTM_Receiving_Application.Module_Core.Helpers.Database;

/// <summary>
/// Manages database connection strings for different environments.
/// Call <see cref="Initialize"/> once at startup before any DAO is used.
/// Credentials are loaded from appsettings.json / environment variables — never hardcoded.
/// </summary>
public static class Helper_Database_Variables
{
    private static string _productionConnectionString = string.Empty;
    private static string _testConnectionString = string.Empty;
    private static string _inforVisualConnectionString = string.Empty;

    /// <summary>
    /// Production MySQL connection string. Non-empty after <see cref="Initialize"/> is called.
    /// </summary>
    public static string ProductionConnectionString => _productionConnectionString;

    /// <summary>
    /// Test MySQL connection string. Non-empty after <see cref="Initialize"/> is called.
    /// </summary>
    public static string TestConnectionString => _testConnectionString;

    /// <summary>
    /// Initializes connection strings from IConfiguration.
    /// Must be called once during application startup before any DAO is resolved.
    /// Reads ConnectionStrings:MySql, ConnectionStrings:MySqlTest, and
    /// ConnectionStrings:InforVisual from appsettings.json.
    /// </summary>
    /// <param name="configuration"></param>
    public static void Initialize(IConfiguration configuration)
    {
        _productionConnectionString = configuration.GetConnectionString("MySql") ?? string.Empty;

        _testConnectionString =
            configuration.GetConnectionString("MySqlTest")
            ?? configuration.GetConnectionString("MySql")
            ?? string.Empty;

        _inforVisualConnectionString =
            configuration.GetConnectionString("InforVisual") ?? string.Empty;
    }

    /// <summary>
    /// Returns the MySQL connection string for the requested environment.
    /// </summary>
    /// <param name="useProduction">True for production, false for test database.</param>
    public static string GetConnectionString(bool useProduction = true)
    {
        return useProduction ? _productionConnectionString : _testConnectionString;
    }

    /// <summary>
    /// Returns the Infor Visual (READ ONLY) connection string.
    /// </summary>
    public static string GetInforVisualConnectionString()
    {
        return _inforVisualConnectionString;
    }
}
