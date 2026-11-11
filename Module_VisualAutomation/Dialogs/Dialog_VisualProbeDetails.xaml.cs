using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Dialogs;

/// <summary>
/// Read-only modal listing every probe from the last run with its verdict and captured values, so the
/// cards on the page can stay short.
/// </summary>
public sealed partial class Dialog_VisualProbeDetails : ContentDialog
{
    public Dialog_VisualProbeDetails(IReadOnlyList<Model_VisualProbeDetail> details, int logCount)
    {
        ArgumentNullException.ThrowIfNull(details);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Details = details;
        LogHeader.Text = $"{details.Count} probes  |  {logCount} log lines on the page";
    }

    /// <summary>Gets the rows shown in the modal.</summary>
    public IReadOnlyList<Model_VisualProbeDetail> Details { get; }
}
