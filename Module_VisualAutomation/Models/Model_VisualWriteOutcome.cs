namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// Result of typing a value into a VISUAL field and reading it back.
/// </summary>
public sealed class Model_VisualWriteOutcome
{
    /// <summary>Gets the macro variable that was written.</summary>
    public required string Key { get; init; }

    /// <summary>Gets the value that was on screen before the write.</summary>
    public string OriginalValue { get; init; } = string.Empty;

    /// <summary>Gets the value the harness typed.</summary>
    public string RequestedValue { get; init; } = string.Empty;

    /// <summary>Gets the value read back from the window after typing.</summary>
    public string ObservedValue { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the observed value matched what was typed.</summary>
    public bool WriteVerified { get; init; }

    /// <summary>Gets a value indicating whether the original value was typed back successfully.</summary>
    public bool RestoreVerified { get; init; }

    /// <summary>Gets the value read back after the restore attempt.</summary>
    public string ValueAfterRestore { get; init; } = string.Empty;
}
