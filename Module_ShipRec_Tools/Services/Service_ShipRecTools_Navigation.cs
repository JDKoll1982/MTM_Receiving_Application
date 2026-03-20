using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Manages the tool registry and routing for the ShipRec Tools module.
/// Built-in tools are registered in RegisterBuiltInTools().
/// Add new tools by calling RegisterTool() or adding to RegisterBuiltInTools().
/// </summary>
public class Service_ShipRecTools_Navigation : IService_ShipRecTools_Navigation
{
    private readonly IService_LoggingUtility _logger;
    private readonly Dictionary<string, Model_ToolDefinition> _toolRegistry = new(
        StringComparer.OrdinalIgnoreCase
    );

    public Service_ShipRecTools_Navigation(IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        RegisterBuiltInTools();
    }

    /// <inheritdoc />
    public IReadOnlyList<Model_ToolDefinition> GetToolsByCategory(Enum_ToolCategory category)
    {
        return _toolRegistry
            .Values.Where(t => t.Category == category && t.IsAvailable)
            .OrderBy(t => t.Title)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<Model_ToolDefinition> GetAllTools()
    {
        return _toolRegistry
            .Values.Where(t => t.IsAvailable)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Title)
            .ToList();
    }

    /// <inheritdoc />
    public Model_ToolDefinition? GetToolByKey(string toolKey)
    {
        return _toolRegistry.TryGetValue(toolKey, out var tool) ? tool : null;
    }

    /// <inheritdoc />
    public void RegisterTool(Model_ToolDefinition tool)
    {
        ArgumentNullException.ThrowIfNull(tool);

        if (string.IsNullOrWhiteSpace(tool.ToolKey))
        {
            throw new ArgumentException("Tool key cannot be empty.", nameof(tool));
        }

        _toolRegistry[tool.ToolKey] = tool;
        _logger.LogInfo($"Registered ShipRec tool: {tool.ToolKey} ({tool.Title})");
    }

    private void RegisterBuiltInTools()
    {
        RegisterTool(
            new Model_ToolDefinition
            {
                ToolKey = "OutsideServiceHistory",
                Title = "Outside Service History",
                Description =
                    "Query vendor history for parts sent to outside service providers. Search by part or vendor.",
                IconGlyph = "\uE7C1",
                Category = Enum_ToolCategory.Lookup,
                IsAvailable = true,
            }
        );
    }
}
