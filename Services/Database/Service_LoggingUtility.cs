using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Services.Database;

/// <summary>
/// Implements file-based logging with daily rotation
/// Logs are written to %APPDATA%\MTM_Receiving_Application\Logs\
/// </summary>
public class Service_LoggingUtility : IService_LoggingUtility
{
    private readonly string _logDirectory;
    private readonly object _lockObject = new object();
    private readonly System.Collections.Concurrent.BlockingCollection<string> _logQueue = new();

    public Service_LoggingUtility()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MTM_Receiving_Application",
            "Logs"
        );

        EnsureLogDirectoryExists();

        // Start background logging thread
        System.Threading.Tasks.Task.Factory.StartNew(ProcessLogQueue,
            System.Threading.CancellationToken.None,
            System.Threading.Tasks.TaskCreationOptions.LongRunning,
            System.Threading.Tasks.TaskScheduler.Default);
    }

    private void ProcessLogQueue()
    {
        foreach (var logEntry in _logQueue.GetConsumingEnumerable())
        {
            try
            {
                string logFilePath = GetCurrentLogFilePath();
                lock (_lockObject)
                {
                    File.AppendAllText(logFilePath, logEntry, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Background logging failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    public void LogInfo(string message, string? context = null)
    {
        WriteLog("INFO", message, null, context);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    public void LogWarning(string message, string? context = null)
    {
        WriteLog("WARNING", message, null, context);
    }

    /// <summary>
    /// Logs an error message with optional exception
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="context"></param>
    public void LogError(string message, Exception? exception = null, string? context = null)
    {
        WriteLog("ERROR", message, exception, context);
    }

    /// <summary>
    /// Logs a critical error message with optional exception
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="context"></param>
    public void LogCritical(string message, Exception? exception = null, string? context = null)
    {
        WriteLog("CRITICAL", message, exception, context);
    }

    /// <summary>
    /// Logs a fatal error message with optional exception
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="context"></param>
    public void LogFatal(string message, Exception? exception = null, string? context = null)
    {
        WriteLog("FATAL", message, exception, context);
    }

    public Task LogInfoAsync(string message, string? context = null)
    {
        LogInfo(message, context);
        return Task.CompletedTask;
    }

    public Task LogErrorAsync(string message, Exception? exception = null, string? context = null)
    {
        LogError(message, exception, context);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the path to today's log file
    /// </summary>
    public string GetCurrentLogFilePath()
    {
        string fileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
        return Path.Combine(_logDirectory, fileName);
    }

    /// <summary>
    /// Ensures the log directory exists and is writable
    /// </summary>
    public bool EnsureLogDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            return true;
        }
        catch (Exception ex)
        {
            // Can't log this error since logging is broken
            System.Diagnostics.Debug.WriteLine($"Failed to create log directory: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Archives old log files (older than specified days)
    /// </summary>
    /// <param name="daysToKeep"></param>
    public int ArchiveOldLogs(int daysToKeep = 30)
    {
        int archivedCount = 0;

        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                return 0;
            }

            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var archiveDirectory = Path.Combine(_logDirectory, "archive");

            foreach (var file in Directory.GetFiles(_logDirectory, "app_*.log"))
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    // Create archive directory if it doesn't exist
                    if (!Directory.Exists(archiveDirectory))
                    {
                        Directory.CreateDirectory(archiveDirectory);
                    }

                    // Move file to archive
                    string archivePath = Path.Combine(archiveDirectory, fileInfo.Name);

                    // If file already exists in archive, delete it first
                    if (File.Exists(archivePath))
                    {
                        File.Delete(archivePath);
                    }

                    File.Move(file, archivePath);
                    archivedCount++;
                }
            }
        }
        catch (Exception ex)
        {
            WriteLog("ERROR", $"Failed to archive old logs: {ex.Message}", ex, "LoggingUtility.ArchiveOldLogs");
        }

        return archivedCount;
    }

    /// <summary>
    /// Core logging method that writes to file
    /// </summary>
    /// <param name="severity"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="context"></param>
    private void WriteLog(string severity, string message, Exception? exception, string? context)
    {
        try
        {
            string logFilePath = GetCurrentLogFilePath();
            var logEntry = new StringBuilder();

            // Timestamp and severity
            logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{severity}] {(context != null ? $"[{context}]" : "")}");
            logEntry.AppendLine($"Message: {message}");

            // Add context information if provided
            if (!string.IsNullOrEmpty(context))
            {
                logEntry.AppendLine($"Context: User={Environment.UserName}, Machine={Environment.MachineName}");
            }

            // Add exception details if provided
            if (exception != null)
            {
                logEntry.AppendLine($"Exception: {exception.GetType().FullName}: {exception.Message}");
                logEntry.AppendLine($"Stack Trace:");
                logEntry.AppendLine(exception.StackTrace);

                // Log inner exceptions
                var innerException = exception.InnerException;
                while (innerException != null)
                {
                    logEntry.AppendLine($"Inner Exception: {innerException.GetType().FullName}: {innerException.Message}");
                    logEntry.AppendLine(innerException.StackTrace);
                    innerException = innerException.InnerException;
                }
            }

            logEntry.AppendLine(); // Blank line between entries

            // Queue for background writing
            _logQueue.Add(logEntry.ToString());

            // Also write to Debug output for development
            System.Diagnostics.Debug.Write(logEntry.ToString());
        }
        catch (Exception ex)
        {
            // Last resort - write to debug output if file logging fails
            System.Diagnostics.Debug.WriteLine($"LOGGING FAILED: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Original message: {message}");
        }
    }
}
