namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The lifecycle state of one working day.
/// </summary>
public enum Enum_ReconciliationDayState
{
    /// <summary>Nothing has been read for this day yet.</summary>
    NotLoaded = 0,

    /// <summary>The day's rows have been read.</summary>
    Loaded = 1,

    /// <summary>The preview is running.</summary>
    Previewing = 2,

    /// <summary>The preview finished and findings exist.</summary>
    Previewed = 3,

    /// <summary>The operator is working through the writable lines.</summary>
    Reviewing = 4,

    /// <summary>Every line is resolved for this session.</summary>
    Complete = 5,

    /// <summary>The day could not be loaded or previewed.</summary>
    Failed = 6,
}
