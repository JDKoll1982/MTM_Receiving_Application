using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Runtime value for a setting.
/// </summary>
public class Model_SettingsValue
{
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public Enum_SettingsScope Scope { get; set; }
    public Enum_SettingsDataType DataType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
    public bool IsSensitive { get; set; }
    public bool IsLocked { get; set; }
    public int? UserId { get; set; }
}
