using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

/// <summary>
/// Bridges custom-field definitions (dunnage_custom_fields, backed by the udc1..udc10
/// columns) with the UI input models (Model_SpecItem / Model_SpecInput) and udc value
/// storage on parts, loads, and sessions.
/// </summary>
public static class Helper_Dunnage_PartSpecs
{
    /// <summary>Number of user-defined columns available per dunnage row.</summary>
    public const int MaxUdcCount = 10;

    /// <summary>Extracts the 10 udc values from a part into a fixed-length array.</summary>
    public static string?[] ExtractUdc(Model_DunnagePart part) =>
        new[] { part.Udc1, part.Udc2, part.Udc3, part.Udc4, part.Udc5, part.Udc6, part.Udc7, part.Udc8, part.Udc9, part.Udc10 };

    /// <summary>Extracts the 10 udc values from a load into a fixed-length array.</summary>
    public static string?[] ExtractUdc(Model_DunnageLoad load) =>
        new[] { load.Udc1, load.Udc2, load.Udc3, load.Udc4, load.Udc5, load.Udc6, load.Udc7, load.Udc8, load.Udc9, load.Udc10 };

    /// <summary>Extracts the 10 udc values from a session into a fixed-length array.</summary>
    public static string?[] ExtractUdc(Model_DunnageSession session) =>
        new[] { session.Udc1, session.Udc2, session.Udc3, session.Udc4, session.Udc5, session.Udc6, session.Udc7, session.Udc8, session.Udc9, session.Udc10 };

    /// <summary>
    /// Builds a slot-keyed array of default values from the type's custom fields.
    /// Values outside slots 1-10 are ignored.
    /// </summary>
    public static string?[] CreateDefaultUdcValues(
        IEnumerable<Model_CustomFieldDefinition> fields
    )
    {
        var values = new string?[MaxUdcCount];
        foreach (var field in fields)
        {
            if (field.DisplayOrder is >= 1 and <= MaxUdcCount)
            {
                values[field.DisplayOrder - 1] = field.DefaultValue;
            }
        }

        return values;
    }

    /// <summary>Copies slot-keyed values onto a part for each custom field slot.</summary>
    public static void ApplyUdcValues(
        Model_DunnagePart part,
        IEnumerable<Model_CustomFieldDefinition> fields,
        string?[] values
    )
    {
        foreach (var field in fields)
        {
            part.SetUdcValue(field.DisplayOrder, GetValueForSlot(values, field.DisplayOrder));
        }
    }

    /// <summary>Copies slot-keyed values onto a load for each custom field slot.</summary>
    public static void ApplyUdcValues(
        Model_DunnageLoad load,
        IEnumerable<Model_CustomFieldDefinition> fields,
        string?[] values
    )
    {
        foreach (var field in fields)
        {
            load.SetUdcValue(field.DisplayOrder, GetValueForSlot(values, field.DisplayOrder));
        }
    }

    /// <summary>Copies slot-keyed values onto a session for each custom field slot.</summary>
    public static void ApplyUdcValues(
        Model_DunnageSession session,
        IEnumerable<Model_CustomFieldDefinition> fields,
        string?[] values
    )
    {
        foreach (var field in fields)
        {
            session.SetUdcValue(field.DisplayOrder, GetValueForSlot(values, field.DisplayOrder));
        }
    }

    /// <summary>Gets the value at the 1-based slot, or null when out of range.</summary>
    public static string? GetValueForSlot(string?[] values, int slot) =>
        slot >= 1 && slot <= values.Length ? values[slot - 1] : null;

    /// <summary>
    /// Builds labeled FieldName -> value pairs for UI display, ordered by slot.
    /// Empty values are omitted.
    /// </summary>
    public static List<KeyValuePair<string, string?>> BuildLabeledPairs(
        IEnumerable<Model_CustomFieldDefinition> fields,
        string?[] udcValues
    ) =>
        fields
            .Where(field => field.DisplayOrder is >= 1 and <= MaxUdcCount)
            .OrderBy(field => field.DisplayOrder)
            .Select(field => new KeyValuePair<string, string?>(
                field.FieldName,
                GetValueForSlot(udcValues, field.DisplayOrder)
            ))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .ToList();

    /// <summary>Converts a custom-field definition to a dialog spec row.</summary>
    public static Model_SpecItem CreateSpecItem(Model_CustomFieldDefinition field) =>
        new()
        {
            Name = field.FieldName,
            DataType = field.FieldType,
            IsRequired = field.IsRequired,
            Unit = field.Unit ?? string.Empty,
            MinValue = field.MinValue.HasValue ? (double)field.MinValue.Value : null,
            MaxValue = field.MaxValue.HasValue ? (double)field.MaxValue.Value : null,
            Choices = field.Choices?.ToList() ?? new List<string>(),
            DefaultValue = field.DefaultValue ?? string.Empty,
        };

    /// <summary>Converts a dialog spec row back to a custom-field definition.</summary>
    public static Model_CustomFieldDefinition CreateDefinition(
        Model_SpecItem spec,
        int displayOrder = 0
    ) =>
        new()
        {
            FieldName = spec.Name,
            FieldType = spec.DataType,
            IsRequired = spec.IsRequired,
            Unit = spec.Unit,
            MinValue = spec.MinValue.HasValue ? (decimal)spec.MinValue.Value : null,
            MaxValue = spec.MaxValue.HasValue ? (decimal)spec.MaxValue.Value : null,
            Choices = spec.Choices?.ToList() ?? new List<string>(),
            DefaultValue = string.IsNullOrWhiteSpace(spec.DefaultValue)
                ? null
                : spec.DefaultValue.Trim(),
            DisplayOrder = displayOrder,
        };

    /// <summary>
    /// Resolves the default value for a spec being added in the type dialog.
    /// Per-type sensible defaults: Choice = first choice, Boolean = false,
    /// Number = 0, Text = blank. A user-typed default overrides the auto value
    /// for Text/Number/Boolean fields.
    /// </summary>
    public static string ResolveSpecDefault(
        string specType,
        string? typedDefault,
        IReadOnlyCollection<string>? choices = null
    )
    {
        var typed = typedDefault?.Trim() ?? string.Empty;

        if (string.Equals(specType?.Trim(), "Choices", StringComparison.OrdinalIgnoreCase))
        {
            return choices?.FirstOrDefault() ?? string.Empty;
        }

        if (string.Equals(specType?.Trim(), "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(typed) ? "false" : typed;
        }

        if (string.Equals(specType?.Trim(), "Number", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(typed) ? "0" : typed;
        }

        return typed;
    }

    /// <summary>Creates a details-entry input row from a definition plus the current value.</summary>
    public static Model_SpecInput CreateSpecInput(
        Model_CustomFieldDefinition field,
        object? value = null
    ) =>
        new()
        {
            SpecName = field.FieldName,
            SpecType = NormalizeSpecType(field.FieldType),
            Value = value,
            Unit = field.Unit,
            IsRequired = field.IsRequired,
            Choices = field.Choices?.ToList() ?? new List<string>(),
        };

    private static string NormalizeSpecType(string? specType)
    {
        return specType?.Trim().ToLowerInvariant() switch
        {
            "number" => "number",
            "boolean" => "boolean",
            "choices" => "choices",
            _ => "text",
        };
    }
}
