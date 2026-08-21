using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Models.Reprint;

namespace MTM_Receiving_Application.Module_Reprint.Models;

/// <summary>
/// Observable grid row for the Reprint page. Wraps a history row and exposes the checkbox state.
/// Rows that are already queued for reprint cannot be selected and render with a red background.
/// </summary>
public partial class Model_ReprintSelectableRow : ObservableObject
{
    private static readonly SolidColorBrush AlreadyQueuedBackground = new(Microsoft.UI.Colors.LightCoral);

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _showDateColumn = true;

    [ObservableProperty]
    private bool _showPartColumn = true;

    [ObservableProperty]
    private bool _showQtyColumn = true;

    [ObservableProperty]
    private bool _showReferenceColumn = true;

    public Model_ReprintSelectableRow(Model_ReprintHistoryRow row)
    {
        Row = row ?? throw new ArgumentNullException(nameof(row));
    }

    public Model_ReprintHistoryRow Row { get; }

    public string HistoryId => Row.HistoryId;

    public DateTime RecordDate => Row.RecordDate;

    public string Part => Row.Part;

    public decimal Quantity => Row.Quantity;

    public string Reference => Row.Reference;

    public bool AlreadyQueued => Row.AlreadyQueued;

    /// <summary>
    /// Already-queued rows render red and get no checkbox.
    /// </summary>
    public bool CanSelect => !Row.AlreadyQueued;

    public Brush? RowBackground => AlreadyQueued ? AlreadyQueuedBackground : null;
}
