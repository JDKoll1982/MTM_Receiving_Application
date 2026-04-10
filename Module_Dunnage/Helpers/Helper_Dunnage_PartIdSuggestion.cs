using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

internal static class Helper_Dunnage_PartIdSuggestion
{
    public static string BuildSuggestedPartId(
        string typeName,
        IEnumerable<KeyValuePair<string, object?>> specValues
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

        foreach (var pair in specValues)
        {
            if (string.Equals(pair.Key, "Notes", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (
                pair.Value is JsonElement element
                && Helper_Dunnage_PartSpecs.TryGetSpecDefinition(element, out _)
            )
            {
                continue;
            }

            if (pair.Value is SpecDefinition)
            {
                continue;
            }

            if (TryGetBoolean(pair.Value, out var boolValue))
            {
                if (boolValue)
                {
                    booleanValues.Add(AbbreviateLabel(pair.Key));
                }

                continue;
            }

            if (TryGetNumber(pair.Value, out var numericValue))
            {
                numericValues.Add(FormatNumber(numericValue));
                continue;
            }

            var textValue = GetString(pair.Value).Trim();
            if (!string.IsNullOrWhiteSpace(textValue))
            {
                textValues.Add(textValue);
            }
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

    private static string GetString(object? rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.True => "Yes",
                JsonValueKind.False => "No",
                JsonValueKind.Number => element.ToString(),
                _ => element.ToString(),
            };
        }

        return rawValue?.ToString() ?? string.Empty;
    }

    private static bool TryGetBoolean(object? rawValue, out bool value)
    {
        if (rawValue is bool booleanValue)
        {
            value = booleanValue;
            return true;
        }

        if (rawValue is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.True)
            {
                value = true;
                return true;
            }

            if (element.ValueKind == JsonValueKind.False)
            {
                value = false;
                return true;
            }

            if (
                element.ValueKind == JsonValueKind.String
                && bool.TryParse(element.GetString(), out var parsedBoolean)
            )
            {
                value = parsedBoolean;
                return true;
            }
        }

        if (rawValue is string text && bool.TryParse(text, out var parsedValue))
        {
            value = parsedValue;
            return true;
        }

        value = false;
        return false;
    }

    private static bool TryGetNumber(object? rawValue, out double value)
    {
        if (rawValue is null)
        {
            value = 0;
            return false;
        }

        if (rawValue is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                value = element.GetDouble();
                return true;
            }

            if (
                element.ValueKind == JsonValueKind.String
                && double.TryParse(
                    element.GetString(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var parsedJsonNumber
                )
            )
            {
                value = parsedJsonNumber;
                return true;
            }
        }

        if (
            double.TryParse(
                rawValue.ToString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsedValue
            )
        )
        {
            value = parsedValue;
            return true;
        }

        value = 0;
        return false;
    }
}
