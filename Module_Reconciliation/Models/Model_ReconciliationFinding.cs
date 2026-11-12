using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One row's verdict. This is what the grid shows, what the detail pane explains
/// and what the write path consumes. It notifies, because accepting a line moves
/// its state and the pane has to follow.
/// </summary>
public sealed partial class Model_ReconciliationFinding : ObservableObject
{
    /// <summary>Exactly one outcome (FR-032).</summary>
    [ObservableProperty]
    private Enum_ReconciliationOutcome _outcome;

    /// <summary>The location the module proposes, or empty when nothing is proposed.</summary>
    [ObservableProperty]
    private string _proposedLocation;

    /// <summary>Whether the proposed destination was derived or typed by an operator.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DestinationSourceText))]
    private Enum_ReconciliationDestinationSource _destinationSource;

    /// <summary>A plain sentence explaining the outcome.</summary>
    [ObservableProperty]
    private string _reason;

    /// <summary>The raw divergence. Present for the surplus and shortfall outcomes.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DifferenceText))]
    private decimal? _difference;

    /// <summary>The evidence bucket, when one applies.</summary>
    [ObservableProperty]
    private Model_ReconciliationDestinationBucket? _bucket;

    /// <summary>The latest movement time shown as pane evidence.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MovementTimeText))]
    private DateTime? _movementAt;

    /// <summary>The Visual user shown as pane evidence.</summary>
    [ObservableProperty]
    private string _movementUserId = string.Empty;

    /// <summary>The finding's lifecycle state.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWritable))]
    private Enum_ReconciliationFindingState _state;

    /// <summary>
    /// Builds a finding and rejects the one shape the spec calls a defect: a
    /// surplus or shortfall outcome that does not carry the raw difference
    /// (FR-033).
    /// </summary>
    /// <param name="row">The line being judged.</param>
    /// <param name="outcome">Exactly one outcome.</param>
    /// <param name="savedLocation">The location currently saved on the row.</param>
    /// <param name="proposedLocation">The location the module proposes, or empty.</param>
    /// <param name="destinationSource">Whether the proposal was derived or typed.</param>
    /// <param name="reason">A plain sentence for the pane and the attention window.</param>
    /// <param name="difference">The raw divergence, required for a surplus or shortfall.</param>
    /// <param name="bucket">The evidence bucket, when one applies.</param>
    /// <exception cref="ArgumentException">
    /// A surplus or shortfall outcome was supplied without a difference.
    /// </exception>
    public Model_ReconciliationFinding(
        Model_ReconciliationRow row,
        Enum_ReconciliationOutcome outcome,
        string savedLocation,
        string proposedLocation,
        Enum_ReconciliationDestinationSource destinationSource,
        string reason,
        decimal? difference,
        Model_ReconciliationDestinationBucket? bucket
    )
    {
        bool requiresDifference =
            outcome
                is Enum_ReconciliationOutcome.ExtraSurplus
                    or Enum_ReconciliationOutcome.NotEnough;

        if (requiresDifference && difference is null)
        {
            throw new ArgumentException(
                $"A {outcome} finding must carry the raw difference.",
                nameof(difference)
            );
        }

        Row = row;
        SavedLocation = savedLocation;
        _outcome = outcome;
        _proposedLocation = proposedLocation;
        _destinationSource = destinationSource;
        _reason = reason;
        _difference = difference;
        _bucket = bucket;
        _state =
            requiresDifference
                ? Enum_ReconciliationFindingState.Unresolved
                : Enum_ReconciliationFindingState.Proposed;
    }

    /// <summary>The line being judged.</summary>
    public Model_ReconciliationRow Row { get; }

    /// <summary>The location that is there now.</summary>
    public string SavedLocation { get; }

    /// <summary>
    /// The evidence this row was judged on, so accepting the line can re-run the
    /// proof over the same material rather than trusting the preview snapshot
    /// (FR-041).
    /// </summary>
    public Model_ReconciliationProofContext? ProofContext { get; set; }

    /// <summary>True only in <see cref="Enum_ReconciliationFindingState.Writable"/>.</summary>
    public bool IsWritable => State is Enum_ReconciliationFindingState.Writable;

    /// <summary>The movement time as the detail pane shows it.</summary>
    public string MovementTimeText =>
        MovementAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>The raw difference as the detail pane shows it.</summary>
    public string DifferenceText =>
        Difference?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Whether the destination was derived or set by hand, as the pane says it.</summary>
    public string DestinationSourceText =>
        DestinationSource is Enum_ReconciliationDestinationSource.Operator
            ? "Manually set"
            : "Derived from Infor Visual";

    /// <summary>The load or skid number as the detail pane shows it.</summary>
    public string LoadOrSkidText =>
        Row.LoadNumber?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}
