namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Lifecycle state for a scanner batch session.
/// </summary>
public enum Enum_ScannerSessionStatus
{
    Draft,
    Ready,
    Running,
    Stopped,
    Completed,
    Failed,
    Archived,
}
