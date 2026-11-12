namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The verdict for one destination bucket, which is part one of the quantity proof (FR-022).
/// </summary>
public sealed class Model_ReconciliationBucketVerdict
{
    /// <summary>The bucket that was judged.</summary>
    public Model_ReconciliationDestinationBucket Bucket { get; init; } = new();

    /// <summary>What the movement delivered.</summary>
    public decimal NetQuantity { get; init; }

    /// <summary>What the rows consumed.</summary>
    public decimal AllocatedQuantity { get; init; }

    /// <summary>The signed remainder, which must be exactly zero.</summary>
    public decimal Remainder => NetQuantity - AllocatedQuantity;

    /// <summary>True when the remainder is exactly zero.</summary>
    public bool ConsumedToZero => Remainder is 0m;

    /// <summary>Copied from the bucket.</summary>
    public bool RanNegative { get; init; }

    /// <summary>
    /// The outcome the affected rows carry when the bucket fails: not enough when
    /// the bucket ran negative or the remainder is negative, otherwise extra
    /// surplus because the movement showed more material than the rows account
    /// for (FR-022, FR-025).
    /// </summary>
    public Enum_ReconciliationOutcome OutcomeWhenFailed =>
        RanNegative || Remainder < 0m
            ? Enum_ReconciliationOutcome.NotEnough
            : Enum_ReconciliationOutcome.ExtraSurplus;

    /// <summary>True when the bucket agrees with its rows and never ran negative.</summary>
    public bool Passed => ConsumedToZero && RanNegative is false;

    /// <summary>
    /// The sentence shown in the detail pane and the attention window when this
    /// bucket fails. Every figure formats with the invariant culture (FR-058).
    /// </summary>
    public string FailureReason
    {
        get
        {
            string location = Bucket.LocationId;

            if (RanNegative)
            {
                return $"Destination {location} ran below zero during the window, so its rows were not corrected.";
            }

            string remainder = Remainder.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            string net = NetQuantity.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            string allocated = AllocatedQuantity.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

            return Remainder > 0m
                ? $"Destination {location} received {net} but its rows account for {allocated}, leaving {remainder} unaccounted for."
                : $"Destination {location} received {net} but its rows account for {allocated}, which is {remainder} too many.";
        }
    }
}
