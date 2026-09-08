using System;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Launches the MTM reference-data SyncTool (Database/SyncTool/sync_reference_data.py)
/// as a child process, streams its output, and returns its exit code.
/// Secrets are never placed on the command line: the MySQL password is supplied to
/// the child through the MTM_SYNC_PASSWORD environment variable, matching the tool's
/// own safety rules.
/// </summary>
public interface IService_SyncToolRunner
{
    /// <summary>
    /// Raised with each chunk of captured process output (stdout and stderr).
    /// Raised on a background thread; marshal to the UI thread before touching UI state.
    /// </summary>
    event Action<string>? OutputReceived;

    /// <summary>Gets the bundled default script path (under the application base directory).</summary>
    string DefaultScriptPath { get; }

    /// <summary>Gets the default python executable used to launch the tool ("python").</summary>
    string DefaultPythonPath { get; }

    /// <summary>Gets the user-configured script path from the local override file, if any.</summary>
    string? ConfiguredScriptPath { get; }

    /// <summary>Gets the user-configured python path from the local override file, if any.</summary>
    string? ConfiguredPythonPath { get; }

    /// <summary>Gets the effective script path (configured value or the bundled default).</summary>
    string EffectiveScriptPath { get; }

    /// <summary>Gets the effective python path (configured value or "python").</summary>
    string EffectivePythonPath { get; }

    /// <summary>
    /// Gets the path of the standalone executable shipped next to the script
    /// (sync_reference_data.exe), or null when it is not present. When present,
    /// the tool runs with no Python interpreter required.
    /// </summary>
    string? BundledExePath { get; }

    /// <summary>Gets a value indicating whether the bundled standalone executable is present.</summary>
    bool IsBundledExeAvailable { get; }

    /// <summary>
    /// Gets a value indicating whether a runnable launch entry is available
    /// (the bundled standalone executable OR the Python script).
    /// </summary>
    bool IsScriptAvailable { get; }

    /// <summary>Refreshes the in-memory configured paths from the local override file.</summary>
    Task RefreshConfiguredPathsAsync();

    /// <summary>Persists the sync-tool launch paths to the local override file.</summary>
    Task SaveToolPathsAsync(string? scriptPath, string? pythonPath);

    /// <summary>
    /// Runs the SyncTool to completion and returns its process exit code.
    /// </summary>
    /// <param name="request">The action and flags describing the invocation.</param>
    /// <param name="password">MySQL password for the run (never written to the command line).</param>
    /// <param name="cancellationToken">Cancels and terminates the child process.</param>
    Task<int> RunAsync(
        Model_SyncToolRunRequest request,
        string password,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Performs a pre-flight requirement check for running the bundled executable:
    /// the exe and the sync policy file must be present. Backup/copy/restore are
    /// pure Python (pymysql), so no external mysql/mysqldump tools are required.
    /// </summary>
    Model_SyncToolReadiness GetReadiness();
}
