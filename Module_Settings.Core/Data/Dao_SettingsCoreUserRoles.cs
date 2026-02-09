using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Data;

/// <summary>
/// DAO for user-role mappings in settings.
/// </summary>
public class Dao_SettingsCoreUserRoles
{
    private readonly string _connectionString;

    public Dao_SettingsCoreUserRoles(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<List<Model_SettingsUserRole>>> GetByUserAsync(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId }
        };

        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_UserRoles_GetByUser",
            reader => new Model_SettingsUserRole
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
                AssignedAt = reader.GetDateTime(reader.GetOrdinal("assigned_at"))
            },
            parameters);
    }

    /// <summary>
    /// Assigns a role to a user in the settings system.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="roleId">The role's ID to assign</param>
    /// <returns>Result indicating success or failure</returns>
    public Task<Model_Dao_Result> AssignRoleAsync(int userId, int roleId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_role_id", roleId }
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SettingsCore_UserRoles_Assign",
            parameters);
    }
}

