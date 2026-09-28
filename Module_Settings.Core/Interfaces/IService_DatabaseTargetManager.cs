using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Manages the MySQL database the application connects to on its next launch.
/// The selection is persisted to the machine-local appsettings.local.json override
/// (never the settings database, which itself lives in the MySQL database being
/// switched) and is applied at the next application start.
/// </summary>
public interface IService_DatabaseTargetManager
{
    /// <summary>Gets the known switchable database targets.</summary>
    IReadOnlyList<Model_DatabaseTarget> Targets { get; }

    /// <summary>Gets the MySQL connection string the application currently uses.</summary>
    string CurrentConnectionString { get; }

    /// <summary>Gets the database the application is currently connected to.</summary>
    string CurrentDatabaseName { get; }

    /// <summary>Gets a display string for the server the application connects to (host:port).</summary>
    string CurrentServerDisplay { get; }

    /// <summary>
    /// Builds a full MySQL connection string for the supplied target by cloning the
    /// current connection string and swapping only the database name.
    /// </summary>
    string BuildConnectionString(Model_DatabaseTarget target);

    /// <summary>Finds the target whose schema matches the supplied database name, if any.</summary>
    Model_DatabaseTarget? FindByDatabaseName(string? databaseName);

    /// <summary>Persists the chosen target into the local override file (applies after restart).</summary>
    Task SaveTargetAsync(Model_DatabaseTarget target);

    /// <summary>Gets the database name that will be used after the next restart (from the override file).</summary>
    Task<string> GetPendingDatabaseNameAsync();

    /// <summary>Indicates whether the saved target differs from the currently connected database.</summary>
    Task<bool> GetIsRestartPendingAsync();

    /// <summary>
    /// Opens a read-only diagnostic connection using <paramref name="connectionString"/>
    /// and reports whether the server and database are reachable.
    /// </summary>
    Task<Model_DatabaseTestResult> TestConnectionAsync(string connectionString);

    /// <summary>
    /// Lists the databases that actually exist on the configured MySQL server
    /// (SHOW DATABASES). Used to only offer targets that can really be selected.
    /// </summary>
    Task<Model_DatabaseEnumeration> EnumerateDatabasesAsync();
}
