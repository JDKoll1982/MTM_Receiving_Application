using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

/// <summary>
/// Dialog for adding or editing a Volvo part from the settings shell.
/// </summary>
public sealed partial class View_Settings_Volvo_PartAddEditDialog : ContentDialog
{
    public Model_VolvoPart? Part { get; private set; }

    public bool IsEditMode { get; private set; }

    public View_Settings_Volvo_PartAddEditDialog()
    {
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
    }

    
    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var desiredWidth = Math.Ceiling(Math.Max(RootGrid.DesiredSize.Width, 500) + 32);
        var availableWidth = Math.Max(500, XamlRoot.Size.Width - 32);
        var availableHeight = Math.Max(360, XamlRoot.Size.Height - 48);

        Width = Math.Min(desiredWidth, availableWidth);
        MinWidth = Math.Min(500, availableWidth);
        MinHeight = Math.Min(360, availableHeight);
        MaxHeight = availableHeight;
    }

    public void InitializeForAdd()
    {
        IsEditMode = false;
        Title = "Add New Volvo Part";
        PartNumberTextBox.IsReadOnly = false;
        EditModeWarning.IsOpen = false;
        PartNumberTextBox.Text = string.Empty;
        QuantityPerSkidNumberBox.Value = 0;
    }

    public void InitializeForEdit(Model_VolvoPart part)
    {
        ArgumentNullException.ThrowIfNull(part);

        IsEditMode = true;
        Title = $"Edit Part: {part.PartNumber}";
        PartNumberTextBox.IsReadOnly = true;
        EditModeWarning.IsOpen = true;
        PartNumberTextBox.Text = part.PartNumber;
        QuantityPerSkidNumberBox.Value = part.QuantityPerSkid;
    }

    private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(PartNumberTextBox.Text))
        {
            args.Cancel = true;
            return;
        }

        Part = new Model_VolvoPart
        {
            PartNumber = PartNumberTextBox.Text.Trim().ToUpperInvariant(),
            QuantityPerSkid = (int)QuantityPerSkidNumberBox.Value,
            IsActive = true,
        };
    }
}
