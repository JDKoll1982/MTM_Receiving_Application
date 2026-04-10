using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

public static class Helper_Dunnage_PartSpecs
{
    public static Dictionary<string, JsonElement> DeserializeRawElements(string? specValuesJson)
    {
        if (string.IsNullOrWhiteSpace(specValuesJson))
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(specValuesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                values[property.Name] = property.Value.Clone();
            }

            return values;
        }
        catch (JsonException)
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static bool TryGetSpecDefinition(object? rawValue, out SpecDefinition definition)
    {
        if (rawValue is SpecDefinition directDefinition)
        {
            definition = directDefinition;
            NormalizeDefinition(definition);
            return true;
        }

        if (rawValue is JsonElement element)
        {
            return TryGetSpecDefinition(element, out definition);
        }

        definition = new SpecDefinition();
        return false;
    }

    public static bool TryGetSpecDefinition(JsonElement element, out SpecDefinition definition)
    {
        definition = new SpecDefinition();
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!LooksLikeSpecDefinition(element))
        {
            return false;
        }

        try
        {
            definition =
                JsonSerializer.Deserialize<SpecDefinition>(element.GetRawText())
                ?? new SpecDefinition();
            NormalizeDefinition(definition);
            return true;
        }
        catch (JsonException)
        {
            definition = new SpecDefinition();
            return false;
        }
    }

    public static object? ConvertJsonElementToPlainObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number when element.TryGetInt64(out var integerValue) => integerValue,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => element.GetRawText(),
        };
    }

    public static Dictionary<string, object?> BuildCombinedSpecPayload(
        IReadOnlyDictionary<string, object?> configuredValues,
        IEnumerable<Model_SpecItem> partSpecificSpecs,
        string notes
    )
    {
        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in configuredValues)
        {
            payload[pair.Key] = pair.Value;
        }

        foreach (var spec in partSpecificSpecs)
        {
            payload[spec.Name] = CreateDefinition(spec);
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            payload["Notes"] = notes.Trim();
        }

        return payload;
    }

    public static Dictionary<string, object> BuildRuntimeValues(
        IReadOnlyDictionary<string, object> configuredValues,
        IReadOnlyDictionary<string, SpecDefinition> partSpecificDefinitions
    )
    {
        var runtimeValues = new Dictionary<string, object>(
            configuredValues,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var pair in partSpecificDefinitions)
        {
            runtimeValues[pair.Key] = GetDefaultRuntimeValue(pair.Value) ?? string.Empty;
        }

        runtimeValues.Remove("Notes");
        return runtimeValues;
    }

    public static object? GetDefaultRuntimeValue(SpecDefinition definition)
    {
        NormalizeDefinition(definition);

        if (!string.IsNullOrWhiteSpace(definition.DefaultValue))
        {
            if (
                string.Equals(definition.DataType, "Number", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(definition.DefaultValue, out var numericDefault)
            )
            {
                return numericDefault;
            }

            if (
                string.Equals(definition.DataType, "Boolean", StringComparison.OrdinalIgnoreCase)
                && bool.TryParse(definition.DefaultValue, out var boolDefault)
            )
            {
                return boolDefault;
            }

            return definition.DefaultValue;
        }

        if (string.Equals(definition.DataType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Empty;
    }

    public static Model_SpecItem CreateSpecItem(string name, SpecDefinition definition)
    {
        NormalizeDefinition(definition);

        return new Model_SpecItem
        {
            Name = name,
            DataType = definition.DataType,
            IsRequired = definition.Required,
            Unit = definition.Unit,
            MinValue = definition.MinValue,
            MaxValue = definition.MaxValue,
            Choices = definition.Choices?.ToList() ?? new List<string>(),
        };
    }

    public static SpecDefinition CreateDefinition(Model_SpecItem spec)
    {
        var definition = new SpecDefinition
        {
            DataType = spec.DataType,
            Required = spec.IsRequired,
            Unit = spec.Unit,
            MinValue = spec.MinValue,
            MaxValue = spec.MaxValue,
            Choices = spec.Choices?.ToList() ?? new List<string>(),
        };

        NormalizeDefinition(definition);
        return definition;
    }

    private static bool LooksLikeSpecDefinition(JsonElement element)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (
                property.NameEquals("dataType")
                || property.NameEquals("type")
                || property.NameEquals("required")
                || property.NameEquals("defaultValue")
                || property.NameEquals("minValue")
                || property.NameEquals("maxValue")
                || property.NameEquals("unit")
                || property.NameEquals("choices")
            )
            {
                return true;
            }
        }

        return false;
    }

    private static void NormalizeDefinition(SpecDefinition definition)
    {
        definition.DataType = string.IsNullOrWhiteSpace(definition.DataType)
            ? "Text"
            : definition.DataType.Trim();
        definition.Unit ??= string.Empty;
        definition.DefaultValue ??= string.Empty;
        definition.Choices ??= new List<string>();
    }
}
