namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// Which physical table a receiving row was read from.
/// </summary>
public enum Enum_ReconciliationRowSource
{
    /// <summary>The active label queue.</summary>
    CurrentLabel = 0,

    /// <summary>The completed receiving history archive.</summary>
    History = 1,
}
