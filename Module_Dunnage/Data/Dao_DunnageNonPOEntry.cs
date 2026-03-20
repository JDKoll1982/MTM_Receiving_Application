using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

/// <summary>
/// Data access object for the dunnage_non_po_entries table.
/// Provides CRUD operations for saved non-PO reference reasons.
/// </summary>
public class Dao_DunnageNonPOEntry
{
    private readonly string _connectionString;

    public Dao_DunnageNonPOEntry(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<List<Model_DunnageNonPOEntry>>> GetAllAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageNonPOEntry>(
            _connectionString,
            "sp_Dunnage_NonPO_GetAll",
            MapFromReader
        );
    }

    public Task<Model_Dao_Result> UpsertAsync(string value, string createdBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_value", value },
            { "p_created_by", createdBy },
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_NonPO_Upsert",
            parameters
        );
    }

    public Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object> { { "p_id", id } };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_NonPO_Delete",
            parameters
        );
    }

    private static Model_DunnageNonPOEntry MapFromReader(IDataReader reader)
    {
        return new Model_DunnageNonPOEntry
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Value = reader.GetString(reader.GetOrdinal("value")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UseCount = reader.GetInt32(reader.GetOrdinal("use_count")),
        };
    }
}
