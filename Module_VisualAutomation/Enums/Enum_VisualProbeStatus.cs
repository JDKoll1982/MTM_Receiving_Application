namespace MTM_Receiving_Application.Module_VisualAutomation.Enums;

/// <summary>
/// Lifecycle state of one automation probe card.
/// </summary>
public enum Enum_VisualProbeStatus
{
    /// <summary>The probe has not been run yet.</summary>
    NotRun = 0,

    /// <summary>The probe is running against the VISUAL window.</summary>
    Running = 1,

    /// <summary>The probe finished and every requirement was met.</summary>
    Passed = 2,

    /// <summary>The probe finished but a requirement was missing or an error was reported.</summary>
    Failed = 3,

    /// <summary>The probe was not part of the last run.</summary>
    Skipped = 4,
}
