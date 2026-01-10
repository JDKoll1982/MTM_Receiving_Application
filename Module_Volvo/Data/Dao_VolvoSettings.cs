using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for Volvo settings operations
/// </summary>
public class Dao_VolvoSettings
{
    private readonly string _connectionString;

    public Dao_VolvoSettings(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Gets a specific setting by key
    /// </summary>
    /// <param name="settingKey"></param>
    public async Task<Model_Dao_Result<Model_VolvoSetting>> GetSettingAsync(string settingKey)
    {
        var parameters = new Dictionary<string, object>
        {
            { "setting_key", settingKey }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_volvo_settings_get",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Gets all settings, optionally filtered by category
    /// </summary>
    /// <param name="category"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoSetting>>> GetAllSettingsAsync(string? category = null)
    {
        var parameters = new Dictionary<string, object>
        {
            { "category", category ?? string.Empty }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_volvo_settings_get_all",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Inserts or updates a setting value
    /// </summary>
    /// <param name="settingKey"></param>
    /// <param name="settingValue"></param>
    /// <param name="modifiedBy"></param>
    public async Task<Model_Dao_Result> UpsertSettingAsync(string settingKey, string settingValue, string modifiedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "setting_key", settingKey },
            { "setting_value", settingValue },
            { "modified_by", modifiedBy }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_settings_upsert",
            parameters
        );
    }

    /// <summary>
    /// Resets a setting to its default value
    /// </summary>
    /// <param name="settingKey"></param>
    /// <param name="modifiedBy"></param>
    public async Task<Model_Dao_Result> ResetSettingAsync(string settingKey, string modifiedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "setting_key", settingKey },
            { "modified_by", modifiedBy }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_settings_reset",
            parameters
        );
    }

    /// <summary>
    /// Maps DataReader to Model_VolvoSetting
    /// </summary>
    /// <param name="reader"></param>
    private static Model_VolvoSetting MapFromReader(IDataReader reader)
    {
        return new Model_VolvoSetting
        {
            SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
            SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
            SettingType = reader.GetString(reader.GetOrdinal("setting_type")),
            Category = reader.GetString(reader.GetOrdinal("category")),
            Description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description")),
            DefaultValue = reader.GetString(reader.GetOrdinal("default_value")),
            MinValue = reader.IsDBNull(reader.GetOrdinal("min_value"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("min_value")),
            MaxValue = reader.IsDBNull(reader.GetOrdinal("max_value"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("max_value")),
            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by"))
                ? null
                : reader.GetString(reader.GetOrdinal("modified_by"))
        };
    }
}
