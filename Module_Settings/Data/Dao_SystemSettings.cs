using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for system settings operations
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_SystemSettings
{
    private readonly string _connectionString;

    public Dao_SystemSettings(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all system settings ordered by category and UI order
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_SystemSetting>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_System_GetAll",
            MapFromReader
        );
    }

    /// <summary>
    /// Retrieves system settings for a specific category
    /// </summary>
    /// <param name="category"></param>
    public async Task<Model_Dao_Result<List<Model_SystemSetting>>> GetByCategoryAsync(string category)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_System_GetByCategory",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Retrieves a single system setting by category and key
    /// </summary>
    /// <param name="category"></param>
    /// <param name="settingKey"></param>
    public async Task<Model_Dao_Result<Model_SystemSetting>> GetByKeyAsync(string category, string settingKey)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category },
            { "p_setting_key", settingKey }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_System_GetByKey",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Updates a system setting value with audit logging
    /// </summary>
    /// <param name="settingId"></param>
    /// <param name="newValue"></param>
    /// <param name="changedBy"></param>
    /// <param name="ipAddress"></param>
    /// <param name="workstationName"></param>
    public async Task<Model_Dao_Result> UpdateValueAsync(
        int settingId,
        string newValue,
        int changedBy,
        string ipAddress,
        string workstationName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_setting_id", settingId },
            { "p_new_value", newValue },
            { "p_changed_by", changedBy },
            { "p_ip_address", ipAddress ?? string.Empty },
            { "p_workstation_name", workstationName ?? string.Empty }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_System_UpdateValue",
            parameters
        );
    }

    /// <summary>
    /// Resets a system setting to its default value
    /// </summary>
    /// <param name="settingId"></param>
    /// <param name="changedBy"></param>
    /// <param name="ipAddress"></param>
    /// <param name="workstationName"></param>
    public async Task<Model_Dao_Result> ResetToDefaultAsync(
        int settingId,
        int changedBy,
        string ipAddress,
        string workstationName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_setting_id", settingId },
            { "p_changed_by", changedBy },
            { "p_ip_address", ipAddress ?? string.Empty },
            { "p_workstation_name", workstationName ?? string.Empty }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_System_ResetToDefault",
            parameters
        );
    }

    /// <summary>
    /// Locks or unlocks a system setting to prevent modification
    /// </summary>
    /// <param name="settingId"></param>
    /// <param name="isLocked"></param>
    /// <param name="changedBy"></param>
    /// <param name="ipAddress"></param>
    /// <param name="workstationName"></param>
    public async Task<Model_Dao_Result> SetLockedAsync(
        int settingId,
        bool isLocked,
        int changedBy,
        string ipAddress,
        string workstationName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_setting_id", settingId },
            { "p_is_locked", isLocked },
            { "p_changed_by", changedBy },
            { "p_ip_address", ipAddress ?? string.Empty },
            { "p_workstation_name", workstationName ?? string.Empty }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_System_SetLocked",
            parameters
        );
    }

    /// <summary>
    /// Maps a database reader row to a Model_SystemSetting object
    /// </summary>
    /// <param name="reader"></param>
    private static Model_SystemSetting MapFromReader(IDataReader reader)
    {
        return new Model_SystemSetting
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Category = reader.GetString(reader.GetOrdinal("category")),
            SubCategory = reader.IsDBNull(reader.GetOrdinal("sub_category")) ? null : reader.GetString(reader.GetOrdinal("sub_category")),
            SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
            SettingName = reader.GetString(reader.GetOrdinal("setting_name")),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            SettingValue = reader.IsDBNull(reader.GetOrdinal("setting_value")) ? null : reader.GetString(reader.GetOrdinal("setting_value")),
            DefaultValue = reader.IsDBNull(reader.GetOrdinal("default_value")) ? null : reader.GetString(reader.GetOrdinal("default_value")),
            DataType = reader.GetString(reader.GetOrdinal("data_type")),
            Scope = reader.GetString(reader.GetOrdinal("scope")),
            PermissionLevel = reader.GetString(reader.GetOrdinal("permission_level")),
            IsLocked = reader.GetBoolean(reader.GetOrdinal("is_locked")),
            IsSensitive = reader.GetBoolean(reader.GetOrdinal("is_sensitive")),
            ValidationRules = reader.IsDBNull(reader.GetOrdinal("validation_rules")) ? null : reader.GetString(reader.GetOrdinal("validation_rules")),
            UiControlType = reader.GetString(reader.GetOrdinal("ui_control_type")),
            UiOrder = reader.GetInt32(reader.GetOrdinal("ui_order")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetInt32(reader.GetOrdinal("updated_by"))
        };
    }
}
