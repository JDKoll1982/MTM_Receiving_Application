namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// Read-only input to the residual check for one part at one location.
/// The balance as of the end of the strict window is
/// <see cref="OnHand"/> minus <see cref="MovementAfterWindow"/> (FR-023, decision D-02).
/// </summary>
public sealed class Model_ReconciliationLocationBalance
{
    /// <summary>The part the balance belongs to.</summary>
    public string PartId { get; init; } = string.Empty;

    /// <summary>The warehouse the location sits in.</summary>
    public string WarehouseId { get; init; } = string.Empty;

    /// <summary>The location the balance belongs to.</summary>
    public string LocationId { get; init; } = string.Empty;

    /// <summary>The live on-hand quantity at query time.</summary>
    public decimal OnHand { get; init; }

    /// <summary>The signed net movement at the location after the strict window.</summary>
    public decimal MovementAfterWindow { get; init; }

    /// <summary>The reconstructed balance, which is what the residual must equal.</summary>
    public decimal BalanceAsOf => OnHand - MovementAfterWindow;
}
