using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuickAddPartDialog : ContentDialog
{
    public string PartId { get; private set; } = string.Empty;
    public int TypeId { get; private set; }
    public string TypeName { get; private set; } = string.Empty;
    public string SpecValuesJson { get; private set; } = string.Empty;

    private readonly List<Model_DunnageSpec> _specs;
    private readonly Dictionary<string, Control> _specInputs = new();

    public View_Dunnage_QuickAddPartDialog(int typeId, string typeName, List<Model_DunnageSpec> specs)
    {
        InitializeComponent();

        TypeId = typeId;
        TypeName = typeName;
        TypeNameTextBlock.Text = typeName;
        _specs = specs;

        GenerateSpecFields();
        UpdatePartId();
    }

    private void GenerateSpecFields()
    {
        if (_specs == null || _specs.Count == 0)
        {
            return;
        }

        foreach (var spec in _specs)
        {
            // Skip dimensions as they are handled by the static fields
            if (string.Equals(spec.SpecKey, "Width", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(spec.SpecKey, "Height", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(spec.SpecKey, "Depth", System.StringComparison.OrdinalIgnoreCase))
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
                // Intentionally empty - fallback to null
            }

            if (def == null)
            {
                def = new SpecDefinition { DataType = "Text" }; // Fallback
            }

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

            var label = new TextBlock
            {
                Text = labelText,
                Style = (Style?)Application.Current.Resources["CaptionTextBlockStyle"]
            };
            stackPanel.Children.Add(label);

            Control? inputControl = null;

            if (string.Equals(def.DataType, "Number", System.StringComparison.OrdinalIgnoreCase))
            {
                var numberBox = new NumberBox
                {
                    PlaceholderText = "0",
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
                };
                if (def.MinValue.HasValue)
                {
                    numberBox.Minimum = def.MinValue.Value;
                }

                if (def.MaxValue.HasValue)
                {
                    numberBox.Maximum = def.MaxValue.Value;
                }

                numberBox.ValueChanged += (s, e) => UpdatePartId();
                inputControl = numberBox;
            }
            else if (string.Equals(def.DataType, "Boolean", System.StringComparison.OrdinalIgnoreCase))
            {
                var checkBox = new CheckBox
                {
                    Content = "Yes"
                };
                checkBox.Checked += (s, e) => UpdatePartId();
                checkBox.Unchecked += (s, e) => UpdatePartId();
                inputControl = checkBox;
            }
            else // Text
            {
                var textBox = new TextBox
                {
                    PlaceholderText = $"Enter {spec.SpecKey.ToLower()}",
                    MaxLength = 100
                };
                textBox.TextChanged += (s, e) => UpdatePartId();
                inputControl = textBox;
            }

            stackPanel.Children.Add(inputControl);
            DynamicSpecsPanel.Children.Add(stackPanel);
            _specInputs[spec.SpecKey] = inputControl;
        }
    }

    private void OnDimensionChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdatePartId();
    }

    private void UpdatePartId()
    {
        var parts = new List<string>();

        // 1. Type name
        parts.Add(TypeName);

        // 2. Text specs (if any)
        var textSpecs = new List<string>();
        foreach (var kvp in _specInputs)
        {
            if (kvp.Value is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
            {
                textSpecs.Add(tb.Text.Trim());
            }
        }

        // 3. Number specs in parentheses with x separator
        var numbers = new List<double>();
        if (!double.IsNaN(WidthNumberBox.Value) && WidthNumberBox.Value > 0)
        {
            numbers.Add(WidthNumberBox.Value);
        }

        if (!double.IsNaN(HeightNumberBox.Value) && HeightNumberBox.Value > 0)
        {
            numbers.Add(HeightNumberBox.Value);
        }

        if (!double.IsNaN(DepthNumberBox.Value) && DepthNumberBox.Value > 0)
        {
            numbers.Add(DepthNumberBox.Value);
        }

        // Add other number specs from dynamic inputs
        foreach (var kvp in _specInputs)
        {
            if (kvp.Value is NumberBox nb && !double.IsNaN(nb.Value) && nb.Value > 0)
            {
                numbers.Add(nb.Value);
            }
        }

        if (numbers.Count > 0)
        {
            // Format numbers: remove decimals if whole numbers
            var formattedNumbers = numbers.Select(n =>
                n == Math.Floor(n) ? ((int)n).ToString() : n.ToString("0.##")
            );
            parts.Add($"({string.Join("x", formattedNumbers)})");
        }

        // 4. Boolean specs (only if true, abbreviated if >2 words)
        var boolSpecs = new List<string>();
        foreach (var kvp in _specInputs)
        {
            if (kvp.Value is CheckBox cb && cb.IsChecked == true)
            {
                var specName = kvp.Key;
                var words = specName.Split(new[] { ' ', '_' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 2)
                {
                    // Abbreviate: use first letter of each word
                    boolSpecs.Add(string.Join("", words.Select(w => char.ToUpper(w[0]))));
                }
                else
                {
                    // Use full name
                    boolSpecs.Add(specName);
                }
            }
        }

        if (boolSpecs.Count > 0)
        {
            parts.Add(string.Join(", ", boolSpecs));
        }

        // Add text specs if any
        if (textSpecs.Count > 0)
        {
            // Insert text specs after type name
            parts.Insert(1, string.Join(", ", textSpecs));
        }

        PartIdTextBox.Text = string.Join(" - ", parts);
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Set Part ID
        PartId = PartIdTextBox.Text.Trim();

        // Build spec values dictionary
        var specValues = new Dictionary<string, object>();

        // Add dynamic specs
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
