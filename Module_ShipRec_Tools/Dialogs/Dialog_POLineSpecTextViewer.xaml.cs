using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Read-only viewer for full PO line specification binary text.
/// </summary>
public sealed partial class Dialog_POLineSpecTextViewer : ContentDialog
{
    public Dialog_POLineSpecTextViewer(Model_Tool_POLineSpecSearchResult row)
    {
        ArgumentNullException.ThrowIfNull(row);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Title = $"PO {row.PONumber} / Line {row.POLineNumber} - Full Spec Text";
        SpecTextEditor.IsReadOnly = false;
        SpecTextEditor.Document.SetText(TextSetOptions.None, row.SpecText ?? string.Empty);
        SpecTextEditor.IsReadOnly = true;
    }

    }
