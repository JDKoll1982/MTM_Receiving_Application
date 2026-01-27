using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for application settings
/// Manages system and user-level configuration settings
/// </summary>
public class Dao_Receiving_Repository_Settings
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_Settings(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a setting by its key
    /// </summary>
    /// <param name="settingKey">Setting key to lookup</param>
    /// <param name="scope">Scope: 'System' or 'User'</param>
    /// <param name="scopeUserId">User ID if scope is 'User'</param>
    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_Setting>> SelectByKeyAsync(
        string settingKey, 
        string scope = "System", 
        string? scopeUserId = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(settingKey);

            var parameters = new Dictionary<string, object>
            {
                { "p_SettingKey", settingKey },
                { "p_Scope", scope },
                { "p_ScopeUserId", (object?)scopeUserId ?? DBNull.Value }
            };

            _logger.LogInfo($"Selecting setting: Key={settingKey}, Scope={scope}");

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Receiving_Settings_SelectByKey",
                MapSetting,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Setting found: {settingKey}");
            }
            else
            {
                _logger.LogWarning($"Setting not found: {settingKey}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByKeyAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_Setting>(
                $"Error selecting setting: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Inserts or updates a setting (UPSERT operation)
    /// </summary>
    /// <param name="setting">Setting to save</param>
    public async Task<Model_Dao_Result> UpsertAsync(Model_Receiving_TableEntitys_Setting setting)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(setting);

            var parameters = new Dictionary<string, object>
            {
                { "p_SettingKey", setting.SettingKey },
                { "p_SettingValue", setting.SettingValue },
                { "p_SettingType", setting.SettingType },
                { "p_Category", (object?)setting.Category ?? DBNull.Value },
                { "p_Description", (object?)setting.Description ?? DBNull.Value },
                { "p_Scope", setting.Scope },
                { "p_ScopeUserId", (object?)setting.ScopeUserId ?? DBNull.Value },
                { "p_ValidValues", (object?)setting.ValidValues ?? DBNull.Value },
                { "p_MinValue", (object?)setting.MinValue ?? DBNull.Value },
                { "p_MaxValue", (object?)setting.MaxValue ?? DBNull.Value },
                { "p_RequiresRestart", setting.RequiresRestart },
                { "p_ModifiedBy", setting.ScopeUserId ?? "System" }
            };

            _logger.LogInfo($"Upserting setting: {setting.SettingKey} = {setting.SettingValue}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Settings_Upsert",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Setting upserted successfully: {setting.SettingKey}");
            }
            else
            {
                _logger.LogError($"Failed to upsert setting: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpsertAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error upserting setting: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps a database reader row to a Setting entity
    /// </summary>
    private static Model_Receiving_TableEntitys_Setting MapSetting(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_Setting
        {
            SettingId = reader.GetInt32(reader.GetOrdinal("SettingId")),
            SettingKey = reader.GetString(reader.GetOrdinal("SettingKey")),
            SettingValue = reader.GetString(reader.GetOrdinal("SettingValue")),
            SettingType = reader.GetString(reader.GetOrdinal("SettingType")),
            Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? null : reader.GetString(reader.GetOrdinal("Category")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Scope = reader.GetString(reader.GetOrdinal("Scope")),
            ScopeUserId = reader.IsDBNull(reader.GetOrdinal("ScopeUserId")) ? null : reader.GetString(reader.GetOrdinal("ScopeUserId")),
            ValidValues = reader.IsDBNull(reader.GetOrdinal("ValidValues")) ? null : reader.GetString(reader.GetOrdinal("ValidValues")),
            MinValue = reader.IsDBNull(reader.GetOrdinal("MinValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MinValue")),
            MaxValue = reader.IsDBNull(reader.GetOrdinal("MaxValue")) ? null : reader.GetDecimal(reader.GetOrdinal("MaxValue")),
            RequiresRestart = reader.GetBoolean(reader.GetOrdinal("RequiresRestart"))
        };
    }
}
