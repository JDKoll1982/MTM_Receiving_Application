using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Core.Data.Application;

/// <summary>
/// DAO for reading and updating the required application version in MySQL.
/// </summary>
public class Dao_SoftwareVersion
{
    private readonly string _connectionString;

    public Dao_SoftwareVersion(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public Task<Model_Dao_Result<Model_SoftwareVersion>> GetCurrentAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_SoftwareVersion_GetCurrent",
            reader => new Model_SoftwareVersion
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                RequiredVersion = reader.GetString(reader.GetOrdinal("required_version")),
                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("updated_by")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            },
            new Dictionary<string, object>()
        );
    }

    public Task<Model_Dao_Result> UpsertAsync(string requiredVersion, string updatedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_required_version", requiredVersion },
            { "p_updated_by", updatedBy },
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_SoftwareVersion_Upsert",
            parameters
        );
    }
}
