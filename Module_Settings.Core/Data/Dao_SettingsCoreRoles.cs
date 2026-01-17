using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Data;

/// <summary>
/// DAO for settings roles.
/// </summary>
public class Dao_SettingsCoreRoles
{
    private readonly string _connectionString;

    public Dao_SettingsCoreRoles(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<List<Model_SettingsRole>>> GetAllAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_Roles_GetAll",
            reader => new Model_SettingsRole
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                RoleName = reader.GetString(reader.GetOrdinal("role_name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            });
    }
}
