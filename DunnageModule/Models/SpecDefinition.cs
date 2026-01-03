using System.Text.Json.Serialization;

namespace MTM_Receiving_Application.DunnageModule.Models;

public class SpecDefinition
{
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "Text"; // Text, Number, Boolean

    [JsonPropertyName("type")]
    public string Type { get => DataType; set => DataType = value; }

    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    [JsonPropertyName("defaultValue")]
    public string DefaultValue { get; set; } = string.Empty;

    // Number specific
    [JsonPropertyName("minValue")]
    public double? MinValue { get; set; }

    [JsonPropertyName("maxValue")]
    public double? MaxValue { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
}
