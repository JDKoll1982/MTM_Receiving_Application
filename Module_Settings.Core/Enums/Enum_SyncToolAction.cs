namespace MTM_Receiving_Application.Module_Settings.Core.Enums;

/// <summary>
/// SyncTool action modes surfaced in the Database Config page. All modes are
/// read-only except <see cref="Execute"/>, which writes to the chosen target
/// (the live core database additionally requires explicit confirmation).
/// </summary>
public enum Enum_SyncToolAction
{
    /// <summary>Drift report + data-sync what-if summary. Read-only.</summary>
    Inspect = 0,

    /// <summary>Data-sync plan summary only. Read-only.</summary>
    WhatIf = 1,

    /// <summary>Write the ordered, reviewable SQL file. No database writes.</summary>
    GenerateSql = 2,

    /// <summary>Apply the sync to the chosen target. Core requires confirmation.</summary>
    Execute = 3,

    /// <summary>
    /// Run the full disposable-copy verification workflow
    /// (make copy -> sync into it -> verify). Read-only with respect to live data.
    /// </summary>
    VerifyCopy = 4,
}
