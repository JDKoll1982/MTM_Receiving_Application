namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Execution state for one scanner batch item.
/// </summary>
public enum Enum_ScannerExecutionState
{
    Waiting,
    Sending,
    Sent,
    Failed,
    Skipped,
}
