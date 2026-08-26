namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Lifecycle state for a scanner current-list session. The legacy <c>Draft</c> status was
/// removed with the Draft feature; a new list starts as <see cref="Ready"/>.
/// </summary>
public enum Enum_ScannerSessionStatus
{
    Ready,
    Running,
    Stopped,
    Completed,
    Failed,
    Archived,
}
