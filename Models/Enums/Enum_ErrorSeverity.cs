namespace MTM_Receiving_Application.Models.Enums;

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
    /// Warning - potential issue but operation continues
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error - operation failed but application continues
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical - serious issue requiring immediate attention
    /// </summary>
    Critical = 3,

    /// <summary>
    /// Fatal - application cannot continue
    /// </summary>
    Fatal = 4
}
