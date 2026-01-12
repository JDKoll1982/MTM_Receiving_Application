using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for settings audit log operations
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_SettingsAuditLog
{
    private readonly string _connectionString;

    public Dao_SettingsAuditLog(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves audit log entries for a setting (most recent first)
    /// </summary>
    /// <param name="settingId"></param>
    /// <param name="limit"></param>
    public async Task<Model_Dao_Result<List<Model_SettingsAuditLog>>> GetAsync(int settingId, int limit)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_setting_id", settingId },
            { "p_limit", limit }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_AuditLog_Get",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Retrieves audit log entries for a specific setting (spec compatibility)
    /// </summary>
    /// <param name="settingId"></param>
    /// <param name="limit"></param>
    public async Task<Model_Dao_Result<List<Model_SettingsAuditLog>>> GetBySettingAsync(int settingId, int limit)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_setting_id", settingId },
            { "p_limit", limit }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_AuditLog_GetBySetting",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Retrieves audit log entries by user (spec compatibility)
    /// </summary>
    /// <param name="changedBy"></param>
    /// <param name="limit"></param>
    public async Task<Model_Dao_Result<List<Model_SettingsAuditLog>>> GetByUserAsync(int changedBy, int limit)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_changed_by", changedBy },
            { "p_limit", limit }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_AuditLog_GetByUser",
            MapFromReader,
            parameters
        );
    }

    private static Model_SettingsAuditLog MapFromReader(IDataReader reader)
    {
        return new Model_SettingsAuditLog
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            SettingId = reader.GetInt32(reader.GetOrdinal("setting_id")),
            Category = reader.GetString(reader.GetOrdinal("category")),
            SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
            SettingName = reader.GetString(reader.GetOrdinal("setting_name")),
            UserSettingId = reader.IsDBNull(reader.GetOrdinal("user_setting_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("user_setting_id")),
            OldValue = reader.IsDBNull(reader.GetOrdinal("old_value"))
                ? null
                : reader.GetString(reader.GetOrdinal("old_value")),
            NewValue = reader.IsDBNull(reader.GetOrdinal("new_value"))
                ? null
                : reader.GetString(reader.GetOrdinal("new_value")),
            ChangeType = reader.GetString(reader.GetOrdinal("change_type")),
            ChangedBy = reader.GetInt32(reader.GetOrdinal("changed_by")),
            ChangedAt = reader.GetDateTime(reader.GetOrdinal("changed_at")),
            IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address"))
                ? null
                : reader.GetString(reader.GetOrdinal("ip_address")),
            WorkstationName = reader.IsDBNull(reader.GetOrdinal("workstation_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("workstation_name"))
        };
    }
}
