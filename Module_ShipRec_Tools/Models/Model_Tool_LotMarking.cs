using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One lot's permanent marking, as stored in the lot_markings table.
/// A lot is the pair (normalized heat number, supplier group).
/// </summary>
public partial class Model_Tool_LotMarking : ObservableObject
{
    [ObservableProperty]
    private int _id;

    /// <summary>
    /// The heat number exactly as it was first captured. Never rewritten by normalization.
    /// </summary>
    [ObservableProperty]
    private string _heatNumber = string.Empty;

    /// <summary>
    /// The matching key. Two heat values that normalize to the same key share one marking.
    /// </summary>
    [ObservableProperty]
    private string _heatNumberNormalized = string.Empty;

    /// <summary>
    /// The supplier group. The official supplier id when it could be resolved, otherwise the
    /// normalized supplier name, or the shared group used when a receipt has no supplier.
    /// Never empty, because a unique key cannot treat one null as equal to another.
    /// </summary>
    [ObservableProperty]
    private string _supplierId = string.Empty;

    /// <summary>
    /// The supplier name as it appeared on the receipt, kept for display and audit.
    /// </summary>
    [ObservableProperty]
    private string _supplierName = string.Empty;

    /// <summary>
    /// The shape emoji stored in shape_code. The glyph itself is the persisted value, so a
    /// marking recorded today keeps its exact appearance no matter how the palette list changes.
    /// </summary>
    [ObservableProperty]
    private string _shapeCode = string.Empty;

    [ObservableProperty]
    private string _colorCode = string.Empty;

    [ObservableProperty]
    private string _markingSource = string.Empty;

    /// <summary>
    /// When set, the resolver returns this marking untouched and probes nothing.
    /// </summary>
    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private string _firstSeenPartNumber = string.Empty;

    /// <summary>
    /// Why the marking was replaced by hand. Required on a manual change.
    /// </summary>
    [ObservableProperty]
    private string _changeReason = string.Empty;

    /// <summary>
    /// The user who last resolved or changed the marking.
    /// </summary>
    [ObservableProperty]
    private string _resolvedBy = string.Empty;

    [ObservableProperty]
    private DateTime? _resolvedAt;

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// How many receiving lines carry this lot, across the queue and history. Filled in when the list
    /// is read and never stored, so it cannot go stale and reading it writes nothing.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineCountText))]
    private int _lineCount;

    /// <summary>
    /// When this lot was last seen on a receiving line. Null when no line carries the lot, which is
    /// the honest answer for a lot whose number has been corrected away.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastLineText))]
    private DateTime? _lastLineDate;

    /// <summary>The line count as it is shown in the list, using a dash when the lot has no lines.</summary>
    public string LineCountText =>
        LineCount == 0 ? "-" : LineCount.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The part numbers that carry this lot, comma separated and in a stable order, or a dash when
    /// none does. Read from the receiving rows rather than stored on the marking, so it is right for
    /// the lots recorded before the enabled part list was consulted.
    /// </summary>
    [ObservableProperty]
    private string _partsText = "-";

    /// <summary>The most recent line's date as it is shown in the list, using a dash when there is none.</summary>
    public string LastLineText =>
        LastLineDate is null
            ? "-"
            : LastLineDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
