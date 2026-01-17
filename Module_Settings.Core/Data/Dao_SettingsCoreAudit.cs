using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Data;

/// <summary>
/// DAO for settings audit entries.
/// </summary>
public class Dao_SettingsCoreAudit
{
    private readonly string _connectionString;

    public Dao_SettingsCoreAudit(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result> InsertAsync(Model_SettingsAuditEntry entry)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_scope", entry.Scope },
            { "p_category", entry.Category },
            { "p_setting_key", entry.SettingKey },
            { "p_old_value", entry.OldValue ?? string.Empty },
            { "p_new_value", entry.NewValue ?? string.Empty },
            { "p_change_type", entry.ChangeType },
            { "p_user_id", entry.UserId ?? 0 },
            { "p_changed_by", entry.ChangedBy },
            { "p_ip_address", entry.IpAddress ?? string.Empty },
            { "p_workstation", entry.WorkstationName ?? string.Empty }
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SettingsCore_Audit_Insert",
            parameters);
    }

    public Task<Model_Dao_Result<List<Model_SettingsAuditEntry>>> GetBySettingAsync(string category, string key)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_category", category },
            { "p_setting_key", key }
        };

        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_Audit_GetBySetting",
            reader => new Model_SettingsAuditEntry
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Scope = reader.GetString(reader.GetOrdinal("scope")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                OldValue = reader.GetString(reader.GetOrdinal("old_value")),
                NewValue = reader.GetString(reader.GetOrdinal("new_value")),
                ChangeType = reader.GetString(reader.GetOrdinal("change_type")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                ChangedBy = reader.GetString(reader.GetOrdinal("changed_by")),
                ChangedAt = reader.GetDateTime(reader.GetOrdinal("changed_at")),
                IpAddress = reader.GetString(reader.GetOrdinal("ip_address")),
                WorkstationName = reader.GetString(reader.GetOrdinal("workstation"))
            },
            parameters);
    }

    public Task<Model_Dao_Result<List<Model_SettingsAuditEntry>>> GetByUserAsync(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_user_id", userId }
        };

        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_SettingsCore_Audit_GetByUser",
            reader => new Model_SettingsAuditEntry
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Scope = reader.GetString(reader.GetOrdinal("scope")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
                OldValue = reader.GetString(reader.GetOrdinal("old_value")),
                NewValue = reader.GetString(reader.GetOrdinal("new_value")),
                ChangeType = reader.GetString(reader.GetOrdinal("change_type")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                ChangedBy = reader.GetString(reader.GetOrdinal("changed_by")),
                ChangedAt = reader.GetDateTime(reader.GetOrdinal("changed_at")),
                IpAddress = reader.GetString(reader.GetOrdinal("ip_address")),
                WorkstationName = reader.GetString(reader.GetOrdinal("workstation"))
            },
            parameters);
    }
}
