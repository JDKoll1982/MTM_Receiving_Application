using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Data;

/// <summary>
/// DAO for user-scoped core settings.
/// </summary>
public class Dao_SettingsCoreUser
{
    private readonly string _connectionString;

    public Dao_SettingsCoreUser(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<Model_UserSetting>> GetByKeyAsync(int userId, string category, string key)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_category", category },
            { "p_key", key }
        };

        return Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_SettingsCore_User_GetByKey",
            reader => new Model_UserSetting
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
                DataType = reader.GetString(reader.GetOrdinal("data_type")),
                UpdatedBy = reader.GetString(reader.GetOrdinal("updated_by")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            },
            parameters);
    }

    public Task<Model_Dao_Result<List<Model_UserSetting>>> GetByCategoryAsync(int userId, string category)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_category", category }
        };

        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_User_GetByCategory",
            reader => new Model_UserSetting
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
                DataType = reader.GetString(reader.GetOrdinal("data_type")),
                UpdatedBy = reader.GetString(reader.GetOrdinal("updated_by")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            },
            parameters);
    }

    public Task<Model_Dao_Result> UpsertAsync(int userId, string category, string key, string value, string dataType, string updatedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_category", category },
            { "p_key", key },
            { "p_value", value },
            { "p_data_type", dataType },
            { "p_updated_by", updatedBy }
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SettingsCore_User_Upsert",
            parameters);
    }

    public Task<Model_Dao_Result> ResetAsync(int userId, string category, string key, string updatedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_category", category },
            { "p_key", key },
            { "p_updated_by", updatedBy }
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SettingsCore_User_Reset",
            parameters);
    }
}
