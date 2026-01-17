using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;

/// <summary>
/// Report containing settings database test results.
/// </summary>
public class Model_SettingsDbTestReport
{
    public bool IsConnected { get; set; }
    public string ConnectionStatus { get; set; } = string.Empty;
    public int ConnectionTimeMs { get; set; }
    public string ServerVersion { get; set; } = string.Empty;
    public int TotalTables { get; set; }
    public int TablesValidated { get; set; }
    public int TotalProcedures { get; set; }
    public int ProceduresValidated { get; set; }
    public int DaosValidated { get; set; }
    public int TotalDaos { get; set; }
    public List<Model_SettingsDbTableResult> TableResults { get; set; } = new();
    public List<Model_SettingsDbProcedureResult> ProcedureResults { get; set; } = new();
    public List<Model_SettingsDbDaoResult> DaoResults { get; set; } = new();
}
