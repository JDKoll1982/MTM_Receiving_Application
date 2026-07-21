namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Failure classification for scanner item send outcomes.
/// </summary>
public enum Enum_ScannerIssueType
{
    None,
    Validation,
    FocusLoss,
    Integrity,
    Timeout,
    AppClosed,
    Unknown,
}
