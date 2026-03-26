using System.Threading;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Coordinates application shutdown state and exposes a shared shutdown token.
/// </summary>
public interface IService_ApplicationShutdown
{
    /// <summary>
    /// Gets a token that is cancelled when application shutdown is requested.
    /// </summary>
    CancellationToken ShutdownToken { get; }

    /// <summary>
    /// Gets a value indicating whether shutdown has been requested.
    /// </summary>
    bool IsShutdownRequested { get; }

    /// <summary>
    /// Gets the requested application exit code.
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    /// Gets the reason associated with the first shutdown request.
    /// </summary>
    string? Reason { get; }

    /// <summary>
    /// Requests application shutdown and cancels the shared shutdown token.
    /// </summary>
    /// <param name="reason">Human-readable shutdown reason.</param>
    /// <param name="exitCode">Process exit code.</param>
    void RequestShutdown(string reason, int exitCode = 0);
}