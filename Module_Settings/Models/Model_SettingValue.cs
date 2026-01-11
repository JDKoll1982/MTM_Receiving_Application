using System;
using System.Text.Json;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Helper model for working with typed setting values
/// Provides type-safe conversion from raw string values
/// </summary>
public class Model_SettingValue
{
    public string RawValue { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Returns the raw string value
    /// </summary>
    public string AsString() => RawValue ?? string.Empty;

    /// <summary>
    /// Parses the value as an integer (returns 0 if invalid)
    /// </summary>
    public int AsInt() => int.TryParse(RawValue, out var val) ? val : 0;

    /// <summary>
    /// Parses the value as a boolean (returns false if invalid)
    /// </summary>
    public bool AsBool() => bool.TryParse(RawValue, out var val) && val;

    /// <summary>
    /// Parses the value as a double (returns 0.0 if invalid)
    /// </summary>
    public double AsDouble() => double.TryParse(RawValue, out var val) ? val : 0.0;

    /// <summary>
    /// Deserializes the value as JSON (returns default(T) if invalid)
    /// </summary>
    public T? AsJson<T>()
    {
        try
        {
            return string.IsNullOrWhiteSpace(RawValue)
                ? default
                : JsonSerializer.Deserialize<T>(RawValue);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Returns the value as a strongly-typed object based on DataType
    /// </summary>
    public object? AsTyped()
    {
        return DataType?.ToLower() switch
        {
            "int" => AsInt(),
            "boolean" => AsBool(),
            "double" => AsDouble(),
            "json" => RawValue,
            _ => AsString()
        };
    }
}
