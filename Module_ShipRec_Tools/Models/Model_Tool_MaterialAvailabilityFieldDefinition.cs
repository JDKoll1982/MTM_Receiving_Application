namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Defines one configurable work-order detail field for the Material Availability Board.
/// </summary>
public sealed class Model_Tool_MaterialAvailabilityFieldDefinition
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public bool DefaultUiVisible { get; set; }

    public bool DefaultPrintVisible { get; set; }

    public bool IsLogicOnly { get; set; }

    public int SortOrder { get; set; }
}
