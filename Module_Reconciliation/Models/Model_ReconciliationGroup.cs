using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The set of receiving rows competing for one pool of movement evidence.
/// The group is also the failure-isolation boundary: a Visual failure marks only
/// this group's findings as <see cref="Enum_ReconciliationOutcome.Error"/> (FR-068).
/// </summary>
public sealed class Model_ReconciliationGroup
{
    /// <summary>The group's identity.</summary>
    public Model_ReconciliationGroupKey Key { get; init; } =
        new(string.Empty, string.Empty, default, string.Empty);

    /// <summary>Every candidate row in the group.</summary>
    public IReadOnlyList<Model_ReconciliationRow> Rows { get; init; } =
        new List<Model_ReconciliationRow>();

    /// <summary>The saved locations, used to seed chained-movement reachability.</summary>
    public IReadOnlySet<string> SourceLocations { get; init; } = new HashSet<string>();

    /// <summary>The group's original receiving location, which anchors the residual check.</summary>
    public string ReceivingLocation { get; init; } = string.Empty;

    /// <summary>The selected movement evidence.</summary>
    public IReadOnlyList<Model_ReconciliationMovement> Movements { get; set; } =
        new List<Model_ReconciliationMovement>();

    /// <summary>The destination buckets after chained resolution.</summary>
    public IReadOnlyList<Model_ReconciliationDestinationBucket> Buckets { get; set; } =
        new List<Model_ReconciliationDestinationBucket>();

    /// <summary>The rows bound to the buckets, which the write path re-proves against.</summary>
    public IReadOnlyList<Model_ReconciliationAllocation> Allocations { get; set; } =
        new List<Model_ReconciliationAllocation>();

    /// <summary>
    /// The balance input read for the receiving location, used by part two of the
    /// quantity proof. Read by the preview composition so the proof itself depends
    /// on no database (FR-023, decision D-02).
    /// </summary>
    public IReadOnlyList<Model_ReconciliationLocationBalance> Balances { get; set; } =
        new List<Model_ReconciliationLocationBalance>();

    /// <summary>Part two of the quantity proof.</summary>
    public Model_ReconciliationSourceResidual? Residual { get; set; }

    /// <summary>Exactly one finding per row.</summary>
    public IReadOnlyList<Model_ReconciliationFinding> Findings { get; set; } =
        new List<Model_ReconciliationFinding>();
}
