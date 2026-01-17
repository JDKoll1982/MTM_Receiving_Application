using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Loads settings definitions from the manifest defaults file.
/// </summary>
public class Service_SettingsManifestProvider : ISettingsManifestProvider
{
    private const string ManifestPath = "Module_Settings.Core/Defaults/settings.manifest.json";

    public IReadOnlyCollection<Model_SettingsDefinition> LoadDefinitions()
    {
        var absolutePath = Path.Combine(AppContext.BaseDirectory, ManifestPath);
        if (!File.Exists(absolutePath))
        {
            return Array.Empty<Model_SettingsDefinition>();
        }

        var json = File.ReadAllText(absolutePath);
        var document = JsonDocument.Parse(json);
        var settings = new List<Model_SettingsDefinition>();

        if (!document.RootElement.TryGetProperty("settings", out var settingsArray))
        {
            return settings;
        }

        foreach (var item in settingsArray.EnumerateArray())
        {
            settings.Add(new Model_SettingsDefinition
            {
                Category = item.GetProperty("category").GetString() ?? string.Empty,
                Key = item.GetProperty("key").GetString() ?? string.Empty,
                DisplayName = item.GetProperty("displayName").GetString() ?? string.Empty,
                DefaultValue = item.GetProperty("defaultValue").GetString() ?? string.Empty,
                DataType = Enum.Parse<Enum_SettingsDataType>(item.GetProperty("dataType").GetString() ?? "String", true),
                Scope = Enum.Parse<Enum_SettingsScope>(item.GetProperty("scope").GetString() ?? "System", true),
                PermissionLevel = Enum.Parse<Enum_SettingsPermissionLevel>(item.GetProperty("permissionLevel").GetString() ?? "User", true),
                IsSensitive = item.GetProperty("isSensitive").GetBoolean(),
                ValidationRules = item.TryGetProperty("validationRules", out var rules) ? rules.GetString() : null
            });
        }

        return settings;
    }
}
