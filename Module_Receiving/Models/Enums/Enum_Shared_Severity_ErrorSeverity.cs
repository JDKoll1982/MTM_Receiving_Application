namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Severity level for validation errors in the receiving workflow.
/// </summary>
public enum Enum_Shared_Severity_ErrorSeverity
{
    /// <summary>
    /// Informational message (no action required).
    /// </summary>
    Info,

    /// <summary>
    /// Warning (user should review but can proceed).
    /// </summary>
    Warning,

    /// <summary>
    /// Error (blocks workflow progression).
    /// </summary>
    Error
}
