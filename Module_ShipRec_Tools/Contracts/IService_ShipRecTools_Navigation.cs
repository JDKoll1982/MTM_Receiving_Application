using System.Collections.Generic;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Navigation service contract for the ShipRec Tools module.
/// Manages the tool registry and provides routing information for tool selection.
/// Add new tools by calling RegisterTool() with a Model_ToolDefinition.
/// </summary>
public interface IService_ShipRecTools_Navigation
{
    /// <summary>
    /// Returns all available tools filtered by the specified category.
    /// </summary>
    IReadOnlyList<Model_ToolDefinition> GetToolsByCategory(Enum_ToolCategory category);

    /// <summary>
    /// Returns all available tools across all categories, sorted by category then title.
    /// </summary>
    IReadOnlyList<Model_ToolDefinition> GetAllTools();

    /// <summary>
    /// Looks up a tool definition by its unique key. Returns null if not found.
    /// </summary>
    Model_ToolDefinition? GetToolByKey(string toolKey);

    /// <summary>
    /// Registers a tool in the module's tool registry.
    /// Replaces any existing tool with the same ToolKey.
    /// </summary>
    void RegisterTool(Model_ToolDefinition tool);
}
