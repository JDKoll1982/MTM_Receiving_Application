namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Reason a scanner run was stopped.
/// </summary>
public enum Enum_ScannerStopReason
{
    None,
    UserStop,
    ValidationFailure,
    AppNotInFocus,
    IntegrityBlock,
    Unknown,
}
