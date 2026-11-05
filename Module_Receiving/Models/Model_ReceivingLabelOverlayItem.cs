using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One receiving tag the user can add a lot marking to, built from a row already in the label queue.
/// </summary>
/// <remarks>
/// The marking shown here is the one already recorded against the row, read rather than recomputed, so
/// the overlay that prints is the same shape and color as the marking every other screen shows for that
/// lot. A row with no marking is still listed, so a user can see why nothing will print for it.
/// </remarks>
public sealed partial class Model_ReceivingLabelOverlayItem : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>The part this tag was printed for.</summary>
    public string PartId { get; init; } = string.Empty;

    /// <summary>The part description, as the tag shows it.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>The heat or lot number, empty when the receipt carried none.</summary>
    public string HeatLotNumber { get; init; } = string.Empty;

    /// <summary>The sequence number of this tag within its load, for telling two apart.</summary>
    public int LabelNumber { get; init; }

    /// <summary>
    /// When the receipt behind this tag was recorded. Carried so the list can be narrowed by date the way
    /// the Heat / Lot Management tool's list is, rather than only ordered.
    /// </summary>
    public DateTime ReceivedDate { get; init; } = DateTime.Today;

    /// <summary>The shape glyph recorded for this lot, empty when it has no marking.</summary>
    public string ShapeCode { get; init; } = string.Empty;

    /// <summary>The color code recorded for this lot, empty when it has no marking.</summary>
    public string ColorCode { get; init; } = string.Empty;

    /// <summary>
    /// The marking in words, e.g. "Red Star", or the reason there is none. Editing this to change the
    /// marking is deliberately not possible here: the overlay repeats what is already recorded.
    /// </summary>
    public string MarkingText { get; init; } = string.Empty;

    /// <summary>True when this tag has a marking, and so is one the overlay can print.</summary>
    public bool HasMarking =>
        !string.IsNullOrWhiteSpace(ShapeCode) || !string.IsNullOrWhiteSpace(ColorCode);

    /// <summary>The shape in words, or empty when the lot carries no shape.</summary>
    public string ShapeName => Model_LotMarkingPalette.FindShape(ShapeCode)?.DisplayName ?? string.Empty;

    /// <summary>The color in words, or empty when the lot carries no color.</summary>
    public string ColorName => Model_LotMarkingPalette.FindColor(ColorCode)?.DisplayName ?? string.Empty;

    /// <summary>When the receipt was recorded, worded the way the rest of the app words a date.</summary>
    public string ReceivedText => ReceivedDate.ToString("MMM d, yyyy");

    private HashSet<string> _visibleColumns = new(
        Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys,
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    /// Tells this row which columns the list is showing. The columns this screen has no data for —
    /// supplier and skid count, which are lot-level facts rather than tag-level ones — stay hidden
    /// whatever is chosen.
    /// </summary>
    public void SetVisibleColumns(IEnumerable<string> keys)
    {
        _visibleColumns = Model_Tool_LotMarkingColumnCatalog.Normalize(keys);

        OnPropertyChanged(nameof(ShowShape));
        OnPropertyChanged(nameof(ShowColor));
        OnPropertyChanged(nameof(ShowHeat));
        OnPropertyChanged(nameof(ShowPart));
        OnPropertyChanged(nameof(ShowReceived));
    }

    private bool IsColumnVisible(string key) =>
        _visibleColumns.Count == 0 || _visibleColumns.Contains(key);

    public bool ShowShape => HasMarking && IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Shape);

    public bool ShowColor => HasMarking && IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Color);

    public bool ShowHeat =>
        HeatLotNumber.Length > 0 && IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Heat);

    public bool ShowPart => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.Parts);

    public bool ShowReceived => IsColumnVisible(Model_Tool_LotMarkingColumnCatalog.LastLine);

    /// <summary>
    /// Identifies this tag across rebuilds, so a tick survives a change to the filter. Built from the
    /// fields that make a tag distinct, because a label number on its own repeats between loads.
    /// </summary>
    public string SelectionKey => $"{PartId}\u001f{HeatLotNumber}\u001f{LabelNumber}";

    /// <summary>
    /// A copy of this tag carrying its own tick state. The cut-out tab works on copies so ticking a lot
    /// there cannot arm a tag on the overlay tab, which is a different printed sheet.
    /// </summary>
    public Model_ReceivingLabelOverlayItem CopyForCutOut(bool isSelected) =>
        new()
        {
            PartId = PartId,
            Description = Description,
            HeatLotNumber = HeatLotNumber,
            LabelNumber = LabelNumber,
            ReceivedDate = ReceivedDate,
            ShapeCode = ShapeCode,
            ColorCode = ColorCode,
            MarkingText = MarkingText,
            IsSelected = isSelected,
            _visibleColumns = new HashSet<string>(
                _visibleColumns,
                StringComparer.OrdinalIgnoreCase
            ),
        };

    /// <summary>What the list shows for this tag on one line.</summary>
    public string Summary =>
        string.IsNullOrWhiteSpace(HeatLotNumber)
            ? $"{PartId} · label {LabelNumber} · {Description}"
            : $"{PartId} · lot {HeatLotNumber} · label {LabelNumber} · {Description}";
}
