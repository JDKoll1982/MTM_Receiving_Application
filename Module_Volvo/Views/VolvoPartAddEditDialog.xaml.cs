using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.Models;

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
        DescriptionTextBox.Text = string.Empty;
        QuantityPerSkidNumberBox.Value = 0;
    }

    /// <summary>
    /// Initialize dialog in Edit mode with existing part data
    /// </summary>
    public void InitializeForEdit(Model_VolvoPart part)
    {
        IsEditMode = true;
        Title = $"Edit Part: {part.PartNumber}";
        PartNumberTextBox.IsReadOnly = true; // Part number cannot be changed
        EditModeWarning.IsOpen = true;
        
        // Pre-fill fields
        PartNumberTextBox.Text = part.PartNumber;
        DescriptionTextBox.Text = part.Description;
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

        if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
        {
            args.Cancel = true;
            return;
        }

        // Create part object
        Part = new Model_VolvoPart
        {
            PartNumber = PartNumberTextBox.Text.Trim().ToUpperInvariant(),
            Description = DescriptionTextBox.Text.Trim(),
            QuantityPerSkid = (int)QuantityPerSkidNumberBox.Value,
            IsActive = true
        };
    }
}
