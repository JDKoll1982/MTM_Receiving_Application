namespace MTM_Receiving_Application.Module_Core.Models.Enums;

/// <summary>
/// Categorizes error severity levels for logging and error handling
/// </summary>
public enum Enum_ErrorSeverity
{
    /// <summary>
    /// Informational message - no action required
    /// </summary>
    Info = 0,

    /// <summary>
    /// Low severity - minor issue
    /// </summary>
    Low = 1,

    /// <summary>
    /// Warning - potential issue but operation continues
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Medium severity - significant issue but application continues
    /// </summary>
    Medium = 3,

    /// <summary>
    /// Error - operation failed but application continues
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical - serious issue requiring immediate attention
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Fatal - application cannot continue
    /// </summary>
    Fatal = 6
}

