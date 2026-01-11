using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

public class Dao_DunnageUserPreference
{
    private readonly string _connectionString;

    public Dao_DunnageUserPreference(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result> UpsertAsync(string userId, string key, string value)
    {
        var parameters = new Dictionary<string, object>
        {
            { "user_id", userId },
            { "pref_key", key },
            { "pref_value", value }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_UserPreferences_Upsert",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(string userId, int count)
    {
        var parameters = new Dictionary<string, object>
        {
            { "user_id", userId },
            { "count", count }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_IconDefinition>(
            _connectionString,
            "sp_Dunnage_UserPreferences_GetRecentIcons",
            reader => new Model_IconDefinition
            {
                IconName = reader.GetString(reader.GetOrdinal("icon_name"))
            },
            parameters
        );
    }
}

