using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data access object for Receiving reusable non-PO entries and per-part defaults.
/// </summary>
public class Dao_ReceivingNonPOEntry
{
    private readonly string _connectionString;

    public Dao_ReceivingNonPOEntry(string connectionString)
    {
        System.ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    public Task<Model_Dao_Result<List<Model_ReceivingNonPOEntry>>> GetAllAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync<Model_ReceivingNonPOEntry>(
            _connectionString,
            "sp_Receiving_NonPO_GetAll",
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
            "sp_Receiving_NonPO_Upsert",
            parameters
        );
    }

    public Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object> { { "p_id", id } };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Receiving_NonPO_Delete",
            parameters
        );
    }

    public Task<Model_Dao_Result<string?>> GetPartDefaultAsync(string partId)
    {
        var parameters = new Dictionary<string, object> { { "p_part_id", partId } };

        return Helper_Database_StoredProcedure.ExecuteSingleAsync<string?>(
            _connectionString,
            "sp_Receiving_NonPO_PartDefault_GetByPartId",
            reader =>
                reader.IsDBNull(reader.GetOrdinal("value"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("value")),
            parameters
        );
    }

    public Task<Model_Dao_Result> UpsertPartDefaultAsync(
        string partId,
        string value,
        string updatedBy
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_id", partId },
            { "p_value", value },
            { "p_updated_by", updatedBy },
        };

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Receiving_NonPO_PartDefault_Upsert",
            parameters
        );
    }

    private static Model_ReceivingNonPOEntry MapFromReader(IDataReader reader)
    {
        return new Model_ReceivingNonPOEntry
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Value = reader.GetString(reader.GetOrdinal("value")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UseCount = reader.GetInt32(reader.GetOrdinal("use_count")),
        };
    }
}
