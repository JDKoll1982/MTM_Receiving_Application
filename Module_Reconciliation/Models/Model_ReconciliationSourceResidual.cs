namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The quantity the group's rows are expected to leave in the receiving location,
/// and whether that quantity agrees with the reconstructed balance (FR-023, FR-024).
/// </summary>
public sealed class Model_ReconciliationSourceResidual
{
    /// <summary>The location the residual must be held in.</summary>
    public string ReceivingLocation { get; init; } = string.Empty;

    /// <summary>The group row total minus the quantity allocated to destination buckets.</summary>
    public decimal ExpectedResidual { get; init; }

    /// <summary>The reconstructed on-hand, or null when Visual returned no balance row.</summary>
    public decimal? BalanceAsOf { get; init; }

    /// <summary>Which read produced the balance, so the evidence pane can say.</summary>
    public string BalanceSource { get; init; } = string.Empty;

    /// <summary>The divergence, or null when no balance was available.</summary>
    public decimal? Difference =>
        BalanceAsOf.HasValue ? ExpectedResidual - BalanceAsOf.Value : null;

    /// <summary>True only when a balance exists and the difference is exactly zero.</summary>
    public bool Agrees => Difference is 0m;

    /// <summary>
    /// The sentence shown when the residual disagrees with the as-of balance.
    /// Every figure formats with the invariant culture (FR-058).
    /// </summary>
    public string DisagreementReason
    {
        get
        {
            string expected = ExpectedResidual.ToString(
                "0.##",
                System.Globalization.CultureInfo.InvariantCulture
            );

            if (BalanceAsOf.HasValue is false)
            {
                return $"The expected residual of {expected} at {ReceivingLocation} could not be checked because Infor Visual returned no balance for that location.";
            }

            string balance = BalanceAsOf.Value.ToString(
                "0.##",
                System.Globalization.CultureInfo.InvariantCulture
            );
            string difference = (Difference ?? 0m).ToString(
                "0.##",
                System.Globalization.CultureInfo.InvariantCulture
            );

            return $"The expected residual of {expected} at {ReceivingLocation} does not equal the balance of {balance} as of the receipt day; the difference is {difference}.";
        }
    }
}
