using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Text.Json;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_QuickAddPartDialog : ContentDialog
{
    public string PartId { get; private set; } = string.Empty;
    public int TypeId { get; private set; }
    public string TypeName { get; private set; } = string.Empty;
    public string SpecValuesJson { get; private set; } = string.Empty;

    public Dunnage_QuickAddPartDialog(int typeId, string typeName)
    {
        InitializeComponent();

        TypeId = typeId;
        TypeName = typeName;
        TypeNameTextBlock.Text = typeName;
    }

    private void OnPartIdChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        PartIdValidationTextBlock.Visibility = string.IsNullOrWhiteSpace(textBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate Part ID
        if (string.IsNullOrWhiteSpace(PartIdTextBox.Text))
        {
            PartIdValidationTextBlock.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        // Set Part ID
        PartId = PartIdTextBox.Text.Trim();

        // Build spec values dictionary
        var specValues = new Dictionary<string, object>();

        // Add dimensions if provided
        if (!double.IsNaN(WidthNumberBox.Value) && WidthNumberBox.Value > 0)
        {
            specValues["Width"] = WidthNumberBox.Value;
        }

        if (!double.IsNaN(HeightNumberBox.Value) && HeightNumberBox.Value > 0)
        {
            specValues["Height"] = HeightNumberBox.Value;
        }

        if (!double.IsNaN(DepthNumberBox.Value) && DepthNumberBox.Value > 0)
        {
            specValues["Depth"] = DepthNumberBox.Value;
        }

        // Add notes if provided
        if (!string.IsNullOrWhiteSpace(NotesTextBox.Text))
        {
            specValues["Notes"] = NotesTextBox.Text.Trim();
        }

        // Serialize to JSON
        if (specValues.Count > 0)
        {
            SpecValuesJson = JsonSerializer.Serialize(specValues);
        }
        else
        {
            SpecValuesJson = "{}";
        }
    }
}
