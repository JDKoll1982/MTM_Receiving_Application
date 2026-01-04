using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service contract for application logging functionality.
/// Implementations must write to file system with daily rotation.
/// </summary>
public interface IService_LoggingUtility
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="context">Optional context information</param>
    public void LogInfo(string message, string? context = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The warning message to log</param>
    /// <param name="context">Optional context information</param>
    public void LogWarning(string message, string? context = null);

    /// <summary>
    /// Logs an error message with optional exception.
    /// </summary>
    /// <param name="message">The error message to log</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="context">Optional context information</param>
    public void LogError(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Logs a critical error message with optional exception.
    /// </summary>
    /// <param name="message">The critical error message to log</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="context">Optional context information</param>
    public void LogCritical(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Logs a fatal error message with optional exception.
    /// Fatal errors indicate the application should terminate.
    /// </summary>
    /// <param name="message">The fatal error message to log</param>
    /// <param name="exception">Optional exception that caused the error</param>
    /// <param name="context">Optional context information</param>
    public void LogFatal(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Logs an informational message asynchronously.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    public Task LogInfoAsync(string message, string? context = null);

    /// <summary>
    /// Logs a warning message asynchronously.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    public Task LogWarningAsync(string message, string? context = null);

    /// <summary>
    /// Logs an error message asynchronously.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="context"></param>
    public Task LogErrorAsync(string message, Exception? exception = null, string? context = null);

    /// <summary>
    /// Gets the path to the current log file.
    /// </summary>
    /// <returns>Full path to today's log file</returns>
    public string GetCurrentLogFilePath();

    /// <summary>
    /// Ensures the log directory exists and is writable.
    /// </summary>
    /// <returns>True if log directory is accessible, false otherwise</returns>
    public bool EnsureLogDirectoryExists();

    /// <summary>
    /// Archives old log files (older than specified days).
    /// </summary>
    /// <param name="daysToKeep">Number of days of logs to keep</param>
    /// <returns>Number of log files archived</returns>
    public int ArchiveOldLogs(int daysToKeep = 30);
}

