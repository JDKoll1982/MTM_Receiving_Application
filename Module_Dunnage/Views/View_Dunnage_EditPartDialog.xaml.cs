using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_EditPartDialog : ContentDialog
{
    public string UpdatedPartId { get; private set; } = string.Empty;
    public string UpdatedSpecValuesJson { get; private set; } = "{}";
    public string UpdatedHomeLocation { get; private set; } = string.Empty;
    public string SelectedInventoryMethod { get; private set; } = "Not Inventoried";

    private readonly Model_DunnagePart _existingPart;
    private readonly List<Model_DunnageSpec> _specs;
    private readonly Dictionary<string, Control> _specInputs = new();
    private readonly string _typeName;

    public View_Dunnage_EditPartDialog(
        Model_DunnagePart existingPart,
        List<Model_DunnageSpec> specs,
        string typeName,
        string inventoryMethod
    )
    {
        InitializeComponent();

        _existingPart = existingPart;
        _specs = specs;
        _typeName = typeName;

        CurrentPartIdTextBlock.Text = existingPart.PartId;
        TypeNameTextBlock.Text = typeName;

        GenerateSpecFields();
        PrePopulateFromExistingPart(inventoryMethod);
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

    private void PrePopulateFromExistingPart(string inventoryMethod)
    {
        PartIdTextBox.Text = _existingPart.PartId;
        HomeLocationTextBox.Text = _existingPart.HomeLocation ?? string.Empty;
        SelectInventoryMethod(inventoryMethod);

        foreach (var pair in _specInputs)
        {
            if (!_existingPart.SpecValuesDict.TryGetValue(pair.Key, out var rawValue))
            {
                continue;
            }

            if (pair.Value is NumberBox numberBox)
            {
                numberBox.Value = GetDouble(rawValue);
            }
            else if (pair.Value is CheckBox checkBox)
            {
                checkBox.IsChecked = GetBool(rawValue);
            }
            else if (pair.Value is ComboBox comboBox)
            {
                comboBox.SelectedItem = GetString(rawValue);
            }
            else if (pair.Value is TextBox textBox)
            {
                textBox.Text = GetString(rawValue);
            }
        }

        if (_existingPart.SpecValuesDict.TryGetValue("Notes", out var notesValue))
        {
            NotesTextBox.Text = GetString(notesValue);
        }
    }

    private void SelectInventoryMethod(string inventoryMethod)
    {
        foreach (var item in InventoryTypeComboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem)
            {
                var content = comboBoxItem.Content?.ToString() ?? string.Empty;
                if (string.Equals(content, inventoryMethod, StringComparison.OrdinalIgnoreCase))
                {
                    InventoryTypeComboBox.SelectedItem = comboBoxItem;
                    return;
                }
            }
        }

        InventoryTypeComboBox.SelectedIndex = 0;
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
            _typeName,
            GetCurrentSpecValues()
        );
    }

    private void SuggestPartIdButton_Click(object sender, RoutedEventArgs e)
    {
        PartIdTextBox.Text = BuildSuggestedPartId();
    }

    private static double GetDouble(object rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String when double.TryParse(element.GetString(), out var parsed) =>
                    parsed,
                _ => double.NaN,
            };
        }

        return Convert.ToDouble(rawValue);
    }

    private static bool GetBool(object rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(element.GetString(), out var parsed)
                    && parsed,
                _ => false,
            };
        }

        return Convert.ToBoolean(rawValue);
    }

    private static string GetString(object rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? string.Empty
                : element.ToString();
        }

        return rawValue?.ToString() ?? string.Empty;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        UpdatedPartId = PartIdTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(UpdatedPartId))
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

        UpdatedSpecValuesJson = specValues.Count > 0 ? JsonSerializer.Serialize(specValues) : "{}";
        UpdatedHomeLocation = HomeLocationTextBox.Text.Trim();
        SelectedInventoryMethod =
            (InventoryTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
            ?? "Not Inventoried";
    }
}
