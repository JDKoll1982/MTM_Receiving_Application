using System;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// One line in the harness activity log shown on the page.
/// </summary>
public sealed class Model_VisualLogEntry
{
    /// <summary>Gets the timestamp shown in the log.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;

    /// <summary>Gets the severity label, for example INFO, WARN, PASS or FAIL.</summary>
    public string Level { get; init; } = "INFO";

    /// <summary>Gets the log message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the formatted line rendered in the log list.</summary>
    public string Display => $"{Timestamp:HH:mm:ss}  {Level,-4}  {Message}";
}
