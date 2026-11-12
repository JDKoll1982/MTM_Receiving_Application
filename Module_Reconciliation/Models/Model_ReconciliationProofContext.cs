using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The evidence a line was judged on, carried on the finding so accepting it can
/// re-run the quantity proof without the page holding the whole day. It deliberately
/// holds no finding, so the reference graph stays a tree (FR-041).
/// </summary>
public sealed class Model_ReconciliationProofContext
{
    /// <summary>The group's identity.</summary>
    public Model_ReconciliationGroupKey Key { get; init; } =
        new(string.Empty, string.Empty, default, string.Empty);

    /// <summary>Where the residual must be held.</summary>
    public string ReceivingLocation { get; init; } = string.Empty;

    /// <summary>The group's rows.</summary>
    public IReadOnlyList<Model_ReconciliationRow> Rows { get; init; } =
        new List<Model_ReconciliationRow>();

    /// <summary>The destination buckets.</summary>
    public IReadOnlyList<Model_ReconciliationDestinationBucket> Buckets { get; init; } =
        new List<Model_ReconciliationDestinationBucket>();

    /// <summary>The rows bound to those buckets.</summary>
    public IReadOnlyList<Model_ReconciliationAllocation> Allocations { get; set; } =
        new List<Model_ReconciliationAllocation>();

    /// <summary>The balance input, refreshed at accept time.</summary>
    public IReadOnlyList<Model_ReconciliationLocationBalance> Balances { get; set; } =
        new List<Model_ReconciliationLocationBalance>();

    /// <summary>Projects the context back to the shape the quantity proof consumes.</summary>
    public Model_ReconciliationGroup ToGroup() =>
        new()
        {
            Key = Key,
            ReceivingLocation = ReceivingLocation,
            Rows = Rows,
            Buckets = Buckets,
            Allocations = Allocations,
            Balances = Balances,
        };
}
