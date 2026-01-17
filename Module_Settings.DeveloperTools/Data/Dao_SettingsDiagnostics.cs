using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Data;

/// <summary>
/// DAO for diagnostics metadata queries via stored procedures.
/// </summary>
public class Dao_SettingsDiagnostics
{
    private readonly string _connectionString;

    public Dao_SettingsDiagnostics(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<List<string>>> GetTablesAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_Meta_GetTables",
            reader => reader.GetString(reader.GetOrdinal("table_name")));
    }

    public Task<Model_Dao_Result<List<string>>> GetStoredProceduresAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_Meta_GetStoredProcedures",
            reader => reader.GetString(reader.GetOrdinal("procedure_name")));
    }
}
