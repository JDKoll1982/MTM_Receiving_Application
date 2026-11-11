using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Dialogs;

/// <summary>
/// Collects the values the probes send to Infor VISUAL. Every value starts blank, and the dialog will not
/// close with the primary button until all of them are filled in.
/// </summary>
public sealed partial class Dialog_VisualProbeInputs : ContentDialog
{
    public Dialog_VisualProbeInputs(Model_VisualProbeInputs? current)
    {
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Inputs = current?.Clone() ?? new Model_VisualProbeInputs();

        BolIdBox.Text = Inputs.BolId;
        VendPackIdBox.Text = Inputs.VendPackId;
        VendFreightIdBox.Text = Inputs.VendFreightId;

        UpdateValidation();
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    /// <summary>Gets the values as they were when the dialog was opened, or as accepted.</summary>
    public Model_VisualProbeInputs Inputs { get; private set; }

    private void OnValueChanged(object sender, TextChangedEventArgs e) => UpdateValidation();

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Inputs = ReadInputs();

        if (Inputs.IsComplete)
        {
            return;
        }

        args.Cancel = true;
        ValidationBar.Message = Inputs.DescribeMissing();
        ValidationBar.IsOpen = true;
    }

    private Model_VisualProbeInputs ReadInputs() =>
        new()
        {
            BolId = BolIdBox.Text ?? string.Empty,
            VendPackId = VendPackIdBox.Text ?? string.Empty,
            VendFreightId = VendFreightIdBox.Text ?? string.Empty,
        };

    private void UpdateValidation()
    {
        if (ValidationBar is null)
        {
            return;
        }

        var missing = ReadInputs().MissingFields();

        IsPrimaryButtonEnabled = missing.Count == 0;
        ValidationBar.Message = missing.Count == 0
            ? string.Empty
            : "Still blank: " + string.Join(", ", missing);
        ValidationBar.IsOpen = missing.Count > 0;
    }
}
