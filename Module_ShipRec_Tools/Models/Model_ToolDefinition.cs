using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Metadata describing a utility tool available in the ShipRec Tools module.
/// Tools are displayed in the selection screen organized by category.
/// </summary>
public class Model_ToolDefinition
{
    /// <summary>
    /// Unique key used to identify the tool in the registry and for navigation.
    /// </summary>
    public string ToolKey { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown on the tool selection card.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Short description shown on the tool selection card.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Segoe Fluent Icons glyph code for the tool card icon.
    /// Default: \uE90F (Manage)
    /// </summary>
    public string IconGlyph { get; set; } = "\uE90F";

    /// <summary>
    /// Category that determines which section of the selection screen the tool appears in.
    /// </summary>
    public Enum_ToolCategory Category { get; set; } = Enum_ToolCategory.Utilities;

    /// <summary>
    /// Controls whether the tool is visible and clickable on the selection screen.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
