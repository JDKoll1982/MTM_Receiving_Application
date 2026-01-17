namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;

/// <summary>
/// Result for a stored procedure validation.
/// </summary>
public class Model_SettingsDbProcedureResult
{
    public string ProcedureName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public int ExecutionTimeMs { get; set; }
    public string Details { get; set; } = string.Empty;
}
