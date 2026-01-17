using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Code-first definition for a core setting.
/// </summary>
public class Model_SettingsDefinition
{
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public Enum_SettingsDataType DataType { get; set; }
    public Enum_SettingsScope Scope { get; set; }
    public Enum_SettingsPermissionLevel PermissionLevel { get; set; }
    public bool IsSensitive { get; set; }
    public string? ValidationRules { get; set; }
}
