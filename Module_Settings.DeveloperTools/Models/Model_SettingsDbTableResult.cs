namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;

/// <summary>
/// Result for a settings table validation.
/// </summary>
public class Model_SettingsDbTableResult
{
    public string TableName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string Details { get; set; } = string.Empty;
}
