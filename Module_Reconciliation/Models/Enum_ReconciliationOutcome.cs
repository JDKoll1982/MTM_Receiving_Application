namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The eight outcomes a reconciliation line may carry.
/// The display text for each member is produced by
/// <c>Converter_ReconciliationOutcomeBrush</c> and nowhere else (FR-032, FR-050).
/// </summary>
public enum Enum_ReconciliationOutcome
{
    /// <summary>The saved location already matches the derived location.</summary>
    Unchanged = 0,

    /// <summary>The saved location differs from the derived location and the line was written.</summary>
    Updated = 1,

    /// <summary>The movement delivered more material than the rows account for.</summary>
    ExtraSurplus = 2,

    /// <summary>The rows demand more material than the movements deliver.</summary>
    NotEnough = 3,

    /// <summary>No qualifying movement was found for the group.</summary>
    NotFound = 4,

    /// <summary>The row was excluded from candidacy or skipped by the operator for this run.</summary>
    Skipped = 5,

    /// <summary>A Visual call or the write procedure failed.</summary>
    Error = 6,

    /// <summary>An operator supplied the destination by hand and the write succeeded.</summary>
    ManuallySet = 7,
}
