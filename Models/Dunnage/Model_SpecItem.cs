using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Dunnage;

/// <summary>
/// UI model for displaying and editing specification items in dialogs
/// </summary>
public class Model_SpecItem
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }

    public string Description
    {
        get
        {
            var desc = DataType;
            var parts = new List<string>();

            if (MinValue.HasValue)
            {
                parts.Add($"Min: {MinValue.Value}");
            }

            if (MaxValue.HasValue)
            {
                parts.Add($"Max: {MaxValue.Value}");
            }

            if (!string.IsNullOrEmpty(Unit))
            {
                parts.Add(Unit);
            }

            if (parts.Count > 0)
            {
                desc += $" ({string.Join(", ", parts)})";
            }

            return desc;
        }
    }

    public Visibility IsRequiredVisibility => IsRequired ? Visibility.Visible : Visibility.Collapsed;
}
