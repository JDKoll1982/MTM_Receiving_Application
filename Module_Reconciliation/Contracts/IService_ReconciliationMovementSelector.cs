using System.Collections.Generic;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Narrows the movements read for a part down to the ones reachable from the
/// group's own evidence (FR-017).
/// </summary>
public interface IService_ReconciliationMovementSelector
{
    /// <summary>
    /// Returns only the movements reachable from the group's seeds: its saved
    /// source locations plus the canonical purchase order keys of its rows.
    /// </summary>
    /// <param name="group">The group whose evidence is being assembled.</param>
    /// <param name="movements">Every movement read for the part.</param>
    IReadOnlyList<Model_ReconciliationMovement> Select(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationMovement> movements
    );
}
