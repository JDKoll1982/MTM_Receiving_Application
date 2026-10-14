using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Confirms filling in the unmarked lots of one part. The dialog is read-only: it restates the
/// counted preview, and the only decision is whether to go ahead.
/// </summary>
/// <remarks>
/// The default button is Close, because this writes to receipts that were already taken. A stray
/// Enter must not reach historical records.
/// </remarks>
public sealed partial class Dialog_LotMarkingBackfillConfirm : ContentDialog
{
    public Dialog_LotMarkingBackfillConfirm(
        string partId,
        Model_Tool_LotMarkingBackfillPreview preview
    )
    {
        ArgumentNullException.ThrowIfNull(preview);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Title = "Fill in unmarked lots";
        PartText.Text = $"Part number {partId} has lots that were saved before it was added to this list.";
        LotsText.Text = preview.DistinctLots == 1
            ? "1 lot will be given a marking."
            : $"{preview.DistinctLots} lots will be given a marking.";
        RowsText.Text = preview.TotalRows == 1
            ? "That covers 1 receiving row."
            : $"That covers {preview.TotalRows} receiving rows.";
    }
}
