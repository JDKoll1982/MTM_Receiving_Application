using System;
using System.Threading;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services;

/// <summary>
/// Tracks application shutdown requests and provides a shared cancellation token.
/// </summary>
public sealed class Service_ApplicationShutdown : IService_ApplicationShutdown, IDisposable
{
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly object _syncRoot = new();

    public CancellationToken ShutdownToken => _shutdownCts.Token;

    public bool IsShutdownRequested { get; private set; }

    public int ExitCode { get; private set; }

    public string? Reason { get; private set; }

    public void RequestShutdown(string reason, int exitCode = 0)
    {
        lock (_syncRoot)
        {
            if (IsShutdownRequested)
            {
                return;
            }

            IsShutdownRequested = true;
            ExitCode = exitCode;
            Reason = reason;
            _shutdownCts.Cancel();
        }
    }

    public void Dispose()
    {
        _shutdownCts.Dispose();
    }
}