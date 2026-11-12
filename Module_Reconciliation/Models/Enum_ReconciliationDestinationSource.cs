namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// Where a finding's proposed destination came from.
/// </summary>
public enum Enum_ReconciliationDestinationSource
{
    /// <summary>The movement evidence derived the destination.</summary>
    Derived = 0,

    /// <summary>An operator typed the destination by hand.</summary>
    Operator = 1,
}
