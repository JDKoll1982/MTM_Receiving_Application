using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.Models;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Dialog for adding or editing a Volvo part
/// </summary>
public sealed partial class VolvoPartAddEditDialog : ContentDialog
{
    public Model_VolvoPart? Part { get; private set; }
    public bool IsEditMode { get; private set; }

    public VolvoPartAddEditDialog()
    {
        InitializeComponent();
    }

    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(
            new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity)
        );

        var desiredWidth = System.Math.Ceiling(
            System.Math.Max(RootGrid.DesiredSize.Width, 500) + 32
        );
        var availableWidth = System.Math.Max(500, XamlRoot.Size.Width - 32);
        var availableHeight = System.Math.Max(360, XamlRoot.Size.Height - 48);

        Width = System.Math.Min(desiredWidth, availableWidth);
        MinWidth = System.Math.Min(500, availableWidth);
        MinHeight = System.Math.Min(360, availableHeight);
        MaxHeight = availableHeight;
    }

    /// <summary>
    /// Initialize dialog in Add mode
    /// </summary>
    public void InitializeForAdd()
    {
        IsEditMode = false;
        Title = "Add New Volvo Part";
        PartNumberTextBox.IsReadOnly = false;
        EditModeWarning.IsOpen = false;

        // Clear fields
        PartNumberTextBox.Text = string.Empty;
        QuantityPerSkidNumberBox.Value = 0;
    }

    /// <summary>
    /// Initialize dialog in Edit mode with existing part data
    /// </summary>
    /// <param name="part"></param>
    public void InitializeForEdit(Model_VolvoPart part)
    {
        IsEditMode = true;
        Title = $"Edit Part: {part.PartNumber}";
        PartNumberTextBox.IsReadOnly = true; // Part number cannot be changed
        EditModeWarning.IsOpen = true;

        // Pre-fill fields
        PartNumberTextBox.Text = part.PartNumber;
        QuantityPerSkidNumberBox.Value = part.QuantityPerSkid;
    }

    private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(PartNumberTextBox.Text))
        {
            args.Cancel = true;
            // In a real implementation, show error message
            return;
        }

        // Create part object
        Part = new Model_VolvoPart
        {
            PartNumber = PartNumberTextBox.Text.Trim().ToUpperInvariant(),
            QuantityPerSkid = (int)QuantityPerSkidNumberBox.Value,
            IsActive = true,
        };
    }
}
