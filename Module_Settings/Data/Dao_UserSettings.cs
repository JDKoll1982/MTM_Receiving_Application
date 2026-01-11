using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for user settings operations (user preference overrides)
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_UserSettings
{
    private readonly string _connectionString;

    public Dao_UserSettings(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves a user setting with fallback to system default
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="category"></param>
    /// <param name="settingKey"></param>
    public async Task<Model_Dao_Result<Model_SettingValue>> GetAsync(int userId, string category, string settingKey)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_category", category },
            { "p_setting_key", settingKey }
        };

        var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_User_Get",
            MapToSettingValue,
            parameters
        );

        return result;
    }

    /// <summary>
    /// Retrieves all user settings for a specific user
    /// </summary>
    /// <param name="userId"></param>
    public async Task<Model_Dao_Result<List<Model_UserSetting>>> GetAllForUserAsync(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_User_GetAllForUser",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Sets a user setting override (creates or updates)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="settingId"></param>
    /// <param name="value"></param>
    public async Task<Model_Dao_Result> SetAsync(int userId, int settingId, string value)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_setting_id", settingId },
            { "p_setting_value", value }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_User_Set",
            parameters
        );
    }

    /// <summary>
    /// Resets a user setting to system default (removes override)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="settingId"></param>
    public async Task<Model_Dao_Result> ResetAsync(int userId, int settingId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_setting_id", settingId }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_User_Reset",
            parameters
        );
    }

    /// <summary>
    /// Resets all user settings for a user to system defaults
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="changedBy"></param>
    public async Task<Model_Dao_Result<int>> ResetAllAsync(int userId, int changedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId },
            { "p_changed_by", changedBy }
        };

        // Execute and return count of reset settings
        var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_User_ResetAll",
            reader => reader.GetInt32(reader.GetOrdinal("reset_count")),
            parameters
        );

        return result;
    }

    /// <summary>
    /// Maps database reader to Model_SettingValue for effective value retrieval
    /// </summary>
    /// <param name="reader"></param>
    private static Model_SettingValue MapToSettingValue(IDataReader reader)
    {
        return new Model_SettingValue
        {
            RawValue = reader.IsDBNull(reader.GetOrdinal("effective_value"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("effective_value")),
            DataType = reader.GetString(reader.GetOrdinal("data_type"))
        };
    }

    /// <summary>
    /// Maps database reader to Model_UserSetting
    /// </summary>
    /// <param name="reader"></param>
    private static Model_UserSetting MapFromReader(IDataReader reader)
    {
        return new Model_UserSetting
        {
            Id = reader.GetInt32(reader.GetOrdinal("setting_id")),
            UserId = -1, // Will be set from context
            SettingId = reader.GetInt32(reader.GetOrdinal("setting_id")),
            SettingValue = reader.IsDBNull(reader.GetOrdinal("user_override"))
                ? null
                : reader.GetString(reader.GetOrdinal("user_override")),
            SystemSetting = new Model_SystemSetting
            {
                Id = reader.GetInt32(reader.GetOrdinal("setting_id")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                SettingName = reader.GetString(reader.GetOrdinal("setting_name")),
                SettingValue = reader.IsDBNull(reader.GetOrdinal("system_default"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("system_default")),
                DataType = reader.GetString(reader.GetOrdinal("data_type")),
                UiControlType = reader.GetString(reader.GetOrdinal("ui_control_type"))
            }
        };
    }
}
