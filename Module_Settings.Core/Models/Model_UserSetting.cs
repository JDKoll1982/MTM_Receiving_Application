using System;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Represents a user-scoped core setting row.
/// </summary>
public class Model_UserSetting
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
