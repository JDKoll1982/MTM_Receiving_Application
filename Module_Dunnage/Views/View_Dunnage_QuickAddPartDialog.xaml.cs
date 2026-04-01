using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuickAddPartDialog : ContentDialog
{
    public string PartId { get; private set; } = string.Empty;
    public int TypeId { get; }
    public string TypeName { get; }
    public string SpecValuesJson { get; private set; } = "{}";
    public string HomeLocation { get; private set; } = string.Empty;
    public string SelectedInventoryMethod { get; private set; } = "Not Inventoried";

    private readonly List<Model_DunnageSpec> _specs;
    private readonly Dictionary<string, Control> _specInputs = new();

    public View_Dunnage_QuickAddPartDialog(
        int typeId,
        string typeName,
        List<Model_DunnageSpec> specs
    )
    {
        InitializeComponent();

        TypeId = typeId;
        TypeName = typeName;
        TypeNameTextBlock.Text = typeName;
        _specs = specs;

        GenerateSpecFields();
        PartIdTextBox.Text = BuildSuggestedPartId();
    }

    private void GenerateSpecFields()
    {
        for (var index = 0; index < _specs.Count; index++)
        {
            var spec = _specs[index];
            var definition = ParseDefinition(spec.SpecValue);
            var panel = CreateSpecPanel(spec.SpecKey, definition, out var inputControl);
            AddPanelToGrid(panel, index);
            _specInputs[spec.SpecKey] = inputControl;
        }
    }

    private SpecDefinition ParseDefinition(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SpecDefinition>(json)
                ?? new SpecDefinition { DataType = "Text" };
        }
        catch
        {
            return new SpecDefinition { DataType = "Text" };
        }
    }

    private StackPanel CreateSpecPanel(
        string specKey,
        SpecDefinition definition,
        out Control inputControl
    )
    {
        var panel = new StackPanel { Spacing = 4 };
        var labelText = specKey;
        if (definition.Required)
        {
            labelText += " *";
        }

        if (!string.IsNullOrWhiteSpace(definition.Unit))
        {
            labelText += $" ({definition.Unit})";
        }

        panel.Children.Add(
            new TextBlock
            {
                Text = labelText,
                Style = (Style?)Application.Current.Resources["CaptionTextBlockStyle"],
            }
        );

        inputControl = CreateInputControl(specKey, definition);
        panel.Children.Add(inputControl);
        return panel;
    }

    private Control CreateInputControl(string specKey, SpecDefinition definition)
    {
        if (string.Equals(definition.DataType, "Number", StringComparison.OrdinalIgnoreCase))
        {
            var numberBox = new NumberBox
            {
                PlaceholderText = "0",
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            };
            if (definition.MinValue.HasValue)
            {
                numberBox.Minimum = definition.MinValue.Value;
            }

            if (definition.MaxValue.HasValue)
            {
                numberBox.Maximum = definition.MaxValue.Value;
            }

            return numberBox;
        }

        if (string.Equals(definition.DataType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return new CheckBox { Content = "Yes" };
        }

        if (string.Equals(definition.DataType, "Choices", StringComparison.OrdinalIgnoreCase))
        {
            var comboBox = new ComboBox
            {
                PlaceholderText = $"Select {specKey.ToLowerInvariant()}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            foreach (var choice in definition.Choices)
            {
                comboBox.Items.Add(choice);
            }

            return comboBox;
        }

        return new TextBox
        {
            PlaceholderText = $"Enter {specKey.ToLowerInvariant()}",
            MaxLength = 100,
        };
    }

    private void AddPanelToGrid(FrameworkElement panel, int index)
    {
        var row = index / 2;
        while (DynamicSpecsGrid.RowDefinitions.Count <= row)
        {
            DynamicSpecsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        Grid.SetRow(panel, row);
        Grid.SetColumn(panel, index % 2);
        DynamicSpecsGrid.Children.Add(panel);
    }

    private Dictionary<string, object?> GetCurrentSpecValues()
    {
        var specValues = new Dictionary<string, object?>();
        foreach (var pair in _specInputs)
        {
            if (pair.Value is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                specValues[pair.Key] = textBox.Text.Trim();
            }
            else if (pair.Value is NumberBox numberBox && !double.IsNaN(numberBox.Value))
            {
                specValues[pair.Key] = numberBox.Value;
            }
            else if (pair.Value is CheckBox checkBox)
            {
                specValues[pair.Key] = checkBox.IsChecked ?? false;
            }
            else if (pair.Value is ComboBox comboBox && comboBox.SelectedItem is string choice)
            {
                specValues[pair.Key] = choice.Trim();
            }
        }

        return specValues;
    }

    private string BuildSuggestedPartId()
    {
        return Helper_Dunnage_PartIdSuggestion.BuildSuggestedPartId(
            TypeName,
            GetCurrentSpecValues()
        );
    }

    private void SuggestPartIdButton_Click(object sender, RoutedEventArgs e)
    {
        PartIdTextBox.Text = BuildSuggestedPartId();
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        PartId = PartIdTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(PartId))
        {
            args.Cancel = true;
            PartIdTextBox.Focus(FocusState.Programmatic);
            return;
        }

        var specValues = GetCurrentSpecValues();
        if (!string.IsNullOrWhiteSpace(NotesTextBox.Text))
        {
            specValues["Notes"] = NotesTextBox.Text.Trim();
        }

        SpecValuesJson = specValues.Count > 0 ? JsonSerializer.Serialize(specValues) : "{}";
        HomeLocation = HomeLocationTextBox.Text.Trim();
        SelectedInventoryMethod =
            (InventoryTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
            ?? "Not Inventoried";
    }
}
