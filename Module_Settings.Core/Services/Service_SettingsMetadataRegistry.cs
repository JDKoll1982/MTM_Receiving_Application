using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Registers and serves core settings definitions.
/// </summary>
public class Service_SettingsMetadataRegistry : ISettingsMetadataRegistry
{
    private readonly Dictionary<string, Model_SettingsDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public Service_SettingsMetadataRegistry(ISettingsManifestProvider manifestProvider)
    {
        foreach (var definition in manifestProvider.LoadDefinitions())
        {
            Register(definition);
        }
    }

    public IReadOnlyCollection<Model_SettingsDefinition> GetAll()
    {
        return _definitions.Values.ToList();
    }

    public Model_SettingsDefinition? GetDefinition(string category, string key)
    {
        return _definitions.TryGetValue(BuildKey(category, key), out var definition)
            ? definition
            : null;
    }

    public void Register(Model_SettingsDefinition definition)
    {
        _definitions[BuildKey(definition.Category, definition.Key)] = definition;
    }

    private static string BuildKey(string category, string key)
    {
        return $"{category}:{key}";
    }
}
