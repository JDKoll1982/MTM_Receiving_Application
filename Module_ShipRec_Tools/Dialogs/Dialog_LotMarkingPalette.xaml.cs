using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Shows the shapes and colors a lot marking can be drawn from. The palette is read-only: it
/// reports what the system currently offers, and is not where a palette entry is chosen.
/// </summary>
public sealed partial class Dialog_LotMarkingPalette : ContentDialog
{
    public Dialog_LotMarkingPalette(
        IReadOnlyList<Model_LotMarkingShape> shapes,
        IReadOnlyList<Model_LotMarkingColor> colors,
        string? summary = null
    )
    {
        ArgumentNullException.ThrowIfNull(shapes);
        ArgumentNullException.ThrowIfNull(colors);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Title = "Palette";
        SummaryText.Text = summary ?? string.Empty;
        ShapesItems.ItemsSource = shapes;
        ColorsItems.ItemsSource = colors;
    }
}
