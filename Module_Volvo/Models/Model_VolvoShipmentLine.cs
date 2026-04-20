using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents an individual part line item within a Volvo shipment
/// Maps to volvo_line_data database table
/// </summary>
public partial class Model_VolvoShipmentLine : ObservableObject
{
    /// <summary>
    /// Auto-increment primary key
    /// </summary>
    [ObservableProperty]
    private int _id;

    /// <summary>
    /// Foreign key to parent shipment
    /// </summary>
    [ObservableProperty]
    private int _shipmentId;

    /// <summary>
    /// Volvo part number (from volvo_masterdata)
    /// Example: V-EMB-500, V-EMB-750
    /// </summary>
    [ObservableProperty]
    private string _partNumber = string.Empty;

    /// <summary>
    /// Optional part description shown in the card header.
    /// </summary>
    [ObservableProperty]
    private string? _partDescription;

    /// <summary>
    /// Warehouse location for this part line.
    /// Auto-populated from Infor Visual when available.
    /// </summary>
    [ObservableProperty]
    private string _location = string.Empty;

    /// <summary>
    /// PO status used for Volvo card styling.
    /// </summary>
    [ObservableProperty]
    private string _poStatus = VolvoLinePoStatus.Pending;

    /// <summary>
    /// Quantity per skid for this part (cached for recalculation)
    /// </summary>
    [ObservableProperty]
    private int _quantityPerSkid;

    /// <summary>
    /// Number of skids actually received (user-entered count)
    /// </summary>
    [ObservableProperty]
    private int _receivedSkidCount;

    /// <summary>
    /// Calculated piece count from component explosion
    /// IMPORTANT: Stored at creation time (historical integrity - doesn't change if master data updates)
    /// </summary>
    [ObservableProperty]
    private int _calculatedPieceCount;

    /// <summary>
    /// Flag indicating if there is a discrepancy between expected and received quantities
    /// If true, ExpectedSkidCount and DiscrepancyNote are populated
    /// </summary>
    [ObservableProperty]
    private bool _hasDiscrepancy = false;

    /// <summary>
    /// Expected skid count from Volvo packlist (NULL if no discrepancy)
    /// Used to calculate difference: ReceivedSkidCount - ExpectedSkidCount
    /// </summary>
    [ObservableProperty]
    private double? _expectedSkidCount;

    /// <summary>
    /// User's note about the discrepancy (NULL if no discrepancy)
    /// </summary>
    [ObservableProperty]
    private string? _discrepancyNote;

    /// <summary>
    /// Tracks whether the row card is currently expanded.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>
    /// Fallback-safe description display for the card header.
    /// </summary>
    public string PartDescriptionDisplay =>
        string.IsNullOrWhiteSpace(PartDescription) ? "(No Description)" : PartDescription.Trim();

    /// <summary>
    /// Fallback-safe calculated piece count display.
    /// </summary>
    public string CalculatedPieceCountDisplay =>
        CalculatedPieceCount >= 0 ? CalculatedPieceCount.ToString() : "N/A";

    /// <summary>
    /// Expected piece count (calculated from ExpectedSkidCount × QuantityPerSkid)
    /// Returns null if no expected count is set
    /// </summary>
    public int? ExpectedPieceCount =>
        ExpectedSkidCount.HasValue && QuantityPerSkid > 0
            ? (int)(ExpectedSkidCount.Value * QuantityPerSkid)
            : null;

    /// <summary>
    /// Calculated difference between received and expected pieces
    /// Positive = more received than expected, Negative = fewer received
    /// Returns null if no expected count is set
    /// </summary>
    public int? PieceDifference =>
        ExpectedPieceCount.HasValue ? CalculatedPieceCount - ExpectedPieceCount.Value : null;

    /// <summary>
    /// Recalculate pieces when received skid count changes
    /// </summary>
    partial void OnReceivedSkidCountChanged(int value)
    {
        CalculatedPieceCount = QuantityPerSkid * value;
        OnPropertyChanged(nameof(PieceDifference));
        OnPropertyChanged(nameof(ExpectedPieceCount));
    }

    /// <summary>
    /// Update calculated values when quantity per skid changes
    /// </summary>
    partial void OnQuantityPerSkidChanged(int value)
    {
        CalculatedPieceCount = ReceivedSkidCount * value;
        OnPropertyChanged(nameof(ExpectedPieceCount));
        OnPropertyChanged(nameof(PieceDifference));
    }

    /// <summary>
    /// Update calculated difference when expected skid count changes
    /// </summary>
    partial void OnExpectedSkidCountChanged(double? value)
    {
        OnPropertyChanged(nameof(ExpectedPieceCount));
        OnPropertyChanged(nameof(PieceDifference));
    }

    partial void OnCalculatedPieceCountChanged(int value)
    {
        OnPropertyChanged(nameof(CalculatedPieceCountDisplay));
        OnPropertyChanged(nameof(PieceDifference));
    }

    partial void OnPoStatusChanged(string value)
    {
        var normalizedValue = VolvoLinePoStatus.NormalizeStorageValue(value);
        if (!string.Equals(value, normalizedValue, StringComparison.Ordinal))
        {
            PoStatus = normalizedValue;
        }
    }

    /// <summary>
    /// Clear discrepancy fields when HasDiscrepancy is unchecked
    /// </summary>
    partial void OnHasDiscrepancyChanged(bool value)
    {
        if (!value)
        {
            ExpectedSkidCount = null;
            DiscrepancyNote = null;
        }
    }
}
