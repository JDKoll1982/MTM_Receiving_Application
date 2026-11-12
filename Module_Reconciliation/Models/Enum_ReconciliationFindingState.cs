namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The lifecycle state of one finding. Only <see cref="Writable"/> may be written.
/// </summary>
public enum Enum_ReconciliationFindingState
{
    /// <summary>Allocation placed the row in a bucket.</summary>
    Proposed = 0,

    /// <summary>Both parts of the quantity proof passed.</summary>
    Proven = 1,

    /// <summary>Proven, and the derived destination differs from the saved location.</summary>
    Writable = 2,

    /// <summary>The write procedure reported success.</summary>
    Written = 3,

    /// <summary>The proof failed, nothing covered the row, or Visual failed.</summary>
    Unresolved = 4,

    /// <summary>The operator pressed Escape. Session scoped.</summary>
    SkippedForRun = 5,
}
