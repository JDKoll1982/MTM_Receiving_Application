using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_EditPartDialog : ContentDialog
{
    public string UpdatedSpecValuesJson { get; private set; } = "{}";
    public string UpdatedHomeLocation { get; private set; } = string.Empty;

    private readonly Model_DunnagePart _existingPart;
    private readonly List<Model_DunnageSpec> _specs;
    private readonly Dictionary<string, Control> _specInputs = new();

    public View_Dunnage_EditPartDialog(Model_DunnagePart existingPart, List<Model_DunnageSpec> specs)
    {
        InitializeComponent();

        _existingPart = existingPart;
        _specs = specs;

        PartIdTextBlock.Text = existingPart.PartId;
        TypeNameTextBlock.Text = existingPart.DunnageTypeName;

        GenerateSpecFields();
        PrePopulateFromExistingPart();
    }

    private void GenerateSpecFields()
    {
        if (_specs == null || _specs.Count == 0)
        {
            return;
        }

        foreach (var spec in _specs)
        {
            // Dimensions handled by static fields
            if (string.Equals(spec.SpecKey, "Width", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(spec.SpecKey, "Height", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(spec.SpecKey, "Depth", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SpecDefinition? def = null;
            try
            {
                def = JsonSerializer.Deserialize<SpecDefinition>(spec.SpecValue);
            }
            catch
            {
                // Intentionally empty — fallback below
            }

            def ??= new SpecDefinition { DataType = "Text" };

            var stackPanel = new StackPanel { Spacing = 4 };

            var labelText = spec.SpecKey;
            if (def.Required)
            {
                labelText += " *";
            }

            if (!string.IsNullOrEmpty(def.Unit))
            {
                labelText += $" ({def.Unit})";
            }

            stackPanel.Children.Add(new TextBlock
            {
                Text = labelText,
                Style = (Style?)Application.Current.Resources["CaptionTextBlockStyle"]
            });

            Control? inputControl = null;

            if (string.Equals(def.DataType, "Number", StringComparison.OrdinalIgnoreCase))
            {
                var numberBox = new NumberBox
                {
                    PlaceholderText = "0",
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
                };
                if (def.MinValue.HasValue) numberBox.Minimum = def.MinValue.Value;
                if (def.MaxValue.HasValue) numberBox.Maximum = def.MaxValue.Value;
                inputControl = numberBox;
            }
            else if (string.Equals(def.DataType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                inputControl = new CheckBox { Content = "Yes" };
            }
            else
            {
                inputControl = new TextBox
                {
                    PlaceholderText = $"Enter {spec.SpecKey.ToLower()}",
                    MaxLength = 100
                };
            }

            stackPanel.Children.Add(inputControl);
            DynamicSpecsPanel.Children.Add(stackPanel);
            _specInputs[spec.SpecKey] = inputControl;
        }
    }

    private void PrePopulateFromExistingPart()
    {
        var dict = _existingPart.SpecValuesDict;

        // Dimensions
        TryPopulateNumberBox(WidthNumberBox, dict, "Width");
        TryPopulateNumberBox(HeightNumberBox, dict, "Height");
        TryPopulateNumberBox(DepthNumberBox, dict, "Depth");

        // Dynamic spec inputs
        foreach (var kvp in _specInputs)
        {
            if (!dict.TryGetValue(kvp.Key, out var rawValue))
            {
                continue;
            }

            if (kvp.Value is NumberBox nb)
            {
                nb.Value = GetDouble(rawValue);
            }
            else if (kvp.Value is CheckBox cb)
            {
                cb.IsChecked = GetBool(rawValue);
            }
            else if (kvp.Value is TextBox tb)
            {
                tb.Text = GetString(rawValue);
            }
        }

        // Home location
        HomeLocationTextBox.Text = _existingPart.HomeLocation ?? string.Empty;

        // Notes (stored in spec_values under "Notes")
        if (dict.TryGetValue("Notes", out var notesVal))
        {
            NotesTextBox.Text = GetString(notesVal);
        }
    }

    private static void TryPopulateNumberBox(NumberBox numberBox, Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var rawValue))
        {
            numberBox.Value = GetDouble(rawValue);
        }
    }

    private static double GetDouble(object rawValue)
    {
        if (rawValue is JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.Number => el.GetDouble(),
                JsonValueKind.String when double.TryParse(el.GetString(), out var d) => d,
                _ => double.NaN
            };
        }

        return Convert.ToDouble(rawValue);
    }

    private static bool GetBool(object rawValue)
    {
        if (rawValue is JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => el.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false,
                _ => false
            };
        }

        return Convert.ToBoolean(rawValue);
    }

    private static string GetString(object rawValue)
    {
        if (rawValue is JsonElement el)
        {
            return el.ValueKind == JsonValueKind.String ? el.GetString() ?? string.Empty : el.ToString();
        }

        return rawValue?.ToString() ?? string.Empty;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var specValues = new Dictionary<string, object>();

        // Dynamic spec fields
        foreach (var kvp in _specInputs)
        {
            if (kvp.Value is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
            {
                specValues[kvp.Key] = tb.Text.Trim();
            }
            else if (kvp.Value is NumberBox nb && !double.IsNaN(nb.Value))
            {
                specValues[kvp.Key] = nb.Value;
            }
            else if (kvp.Value is CheckBox cb)
            {
                specValues[kvp.Key] = cb.IsChecked ?? false;
            }
        }

        // Dimensions
        if (!double.IsNaN(WidthNumberBox.Value) && WidthNumberBox.Value > 0)
            specValues["Width"] = WidthNumberBox.Value;

        if (!double.IsNaN(HeightNumberBox.Value) && HeightNumberBox.Value > 0)
            specValues["Height"] = HeightNumberBox.Value;

        if (!double.IsNaN(DepthNumberBox.Value) && DepthNumberBox.Value > 0)
            specValues["Depth"] = DepthNumberBox.Value;

        // Notes
        if (!string.IsNullOrWhiteSpace(NotesTextBox.Text))
            specValues["Notes"] = NotesTextBox.Text.Trim();

        UpdatedSpecValuesJson = specValues.Count > 0 ? JsonSerializer.Serialize(specValues) : "{}";
        UpdatedHomeLocation = HomeLocationTextBox.Text.Trim();
    }
}
