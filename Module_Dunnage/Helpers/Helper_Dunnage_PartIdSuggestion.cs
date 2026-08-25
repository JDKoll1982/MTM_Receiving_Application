using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

internal static class Helper_Dunnage_PartIdSuggestion
{
    public static string BuildSuggestedPartId(
        string typeName,
        IEnumerable<KeyValuePair<string, string?>> labeledValues
    )
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(typeName))
        {
            parts.Add(typeName.Trim());
        }

        var textValues = new List<string>();
        var numericValues = new List<string>();
        var booleanValues = new List<string>();

        foreach (var pair in labeledValues)
        {
            if (string.Equals(pair.Key, "Notes", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var rawValue = pair.Value;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                continue;
            }

            if (TryGetBoolean(rawValue, out var boolValue))
            {
                if (boolValue)
                {
                    booleanValues.Add(AbbreviateLabel(pair.Key));
                }

                continue;
            }

            if (TryGetNumber(rawValue, out var numericValue))
            {
                numericValues.Add(FormatNumber(numericValue));
                continue;
            }

            textValues.Add(rawValue.Trim());
        }

        if (textValues.Count > 0)
        {
            parts.Add(string.Join(", ", textValues));
        }

        if (numericValues.Count > 0)
        {
            parts.Add($"({string.Join("x", numericValues)})");
        }

        if (booleanValues.Count > 0)
        {
            parts.Add(string.Join(", ", booleanValues));
        }

        return string.Join(" - ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string AbbreviateLabel(string label)
    {
        var words = label.Split(
            [' ', '_', '-'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        if (words.Length > 2)
        {
            return string.Concat(words.Select(word => char.ToUpperInvariant(word[0])));
        }

        return label.Trim();
    }

    private static string FormatNumber(double value)
    {
        return value == Math.Floor(value)
            ? ((int)value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static bool TryGetBoolean(string? rawValue, out bool value)
    {
        if (bool.TryParse(rawValue, out value))
        {
            return true;
        }

        if (rawValue == "1")
        {
            value = true;
            return true;
        }

        if (rawValue == "0")
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }

    private static bool TryGetNumber(string? rawValue, out double value)
    {
        return double.TryParse(
            rawValue,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out value
        );
    }
}
