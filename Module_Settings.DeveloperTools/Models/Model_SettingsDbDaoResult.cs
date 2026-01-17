namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;

/// <summary>
/// Result for a DAO validation.
/// </summary>
public class Model_SettingsDbDaoResult
{
    public string DaoName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string Details { get; set; } = string.Empty;
}
