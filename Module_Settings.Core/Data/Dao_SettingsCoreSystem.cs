using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Data;

/// <summary>
/// DAO for system-scoped core settings.
/// </summary>
public class Dao_SettingsCoreSystem
{
    private readonly string _connectionString;

    public Dao_SettingsCoreSystem(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<Model_CoreSetting>> GetByKeyAsync(string category, string key)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category },
            { "p_key", key }
        };

        return Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_SettingsCore_System_GetByKey",
            reader => new Model_CoreSetting
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
                DataType = reader.GetString(reader.GetOrdinal("data_type")),
                IsSensitive = reader.GetBoolean(reader.GetOrdinal("is_sensitive")),
                IsLocked = reader.GetBoolean(reader.GetOrdinal("is_locked")),
                UpdatedBy = reader.GetString(reader.GetOrdinal("updated_by")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            },
            parameters);
    }

    public Task<Model_Dao_Result<List<Model_CoreSetting>>> GetByCategoryAsync(string category)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category }
        };

        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_System_GetByCategory",
            reader => new Model_CoreSetting
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
                DataType = reader.GetString(reader.GetOrdinal("data_type")),
                IsSensitive = reader.GetBoolean(reader.GetOrdinal("is_sensitive")),
                IsLocked = reader.GetBoolean(reader.GetOrdinal("is_locked")),
                UpdatedBy = reader.GetString(reader.GetOrdinal("updated_by")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
            },
            parameters);
    }

    public Task<Model_Dao_Result> UpsertAsync(string category, string key, string value, string dataType, bool isSensitive, string updatedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category },
            { "p_key", key },
            { "p_value", value },
            { "p_data_type", dataType },
            { "p_is_sensitive", isSensitive },
            { "p_updated_by", updatedBy }
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SettingsCore_System_Upsert",
            parameters);
    }

    public Task<Model_Dao_Result> ResetAsync(string category, string key, string updatedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category },
            { "p_key", key },
            { "p_updated_by", updatedBy }
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SettingsCore_System_Reset",
            parameters);
    }
}
