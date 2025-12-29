using System.Text.Json.Serialization;

namespace MTM_Receiving_Application.Models.Dunnage;

public class SpecDefinition
{
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "Text"; // Text, Number, Boolean

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
