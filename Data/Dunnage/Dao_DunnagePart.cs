using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Data.Dunnage;

public static class Dao_DunnagePart
{
    private static string ConnectionString => Helper_Database_Variables.GetConnectionString();

    public static async Task<DaoResult<List<Model_DunnagePart>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnagePart>(
            ConnectionString,
            "sp_dunnage_parts_get_all",
            MapFromReader
        );
    }

    public static async Task<DaoResult<List<Model_DunnagePart>>> GetByTypeAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnagePart>(
            ConnectionString,
            "sp_dunnage_parts_get_by_type",
            MapFromReader,
            parameters
        );
    }

    public static async Task<DaoResult<Model_DunnagePart>> GetByIdAsync(string partId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_id", partId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnagePart>(
            ConnectionString,
            "sp_dunnage_parts_get_by_id",
            MapFromReader,
            parameters
        );
    }

    public static async Task<DaoResult<int>> InsertAsync(string partId, int typeId, string specValues, string user)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_part_id", partId),
            new MySqlParameter("@p_type_id", typeId),
            new MySqlParameter("@p_spec_values", specValues),
            new MySqlParameter("@p_user", user),
            pNewId
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_dunnage_parts_insert",
            parameters,
            ConnectionString
        );

        if (result.IsSuccess)
        {
            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                return DaoResult<int>.SuccessResult(Convert.ToInt32(pNewId.Value));
            }
            return DaoResult<int>.Failure("Failed to retrieve new ID");
        }

        return DaoResult<int>.Failure(result.ErrorMessage, result.Exception);
    }

    public static async Task<Model_Dao_Result> UpdateAsync(int id, string specValues, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "spec_values", specValues },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            ConnectionString,
            "sp_dunnage_parts_update",
            parameters
        );
    }

    public static async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            ConnectionString,
            "sp_dunnage_parts_delete",
            parameters
        );
    }

    public static async Task<DaoResult<int>> CountTransactionsAsync(string partId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_id", partId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            ConnectionString,
            "sp_dunnage_parts_count_transactions",
            reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
            parameters
        );
    }

    public static async Task<DaoResult<List<Model_DunnagePart>>> SearchAsync(string searchText, int? typeId = null)
    {
        var parameters = new Dictionary<string, object>
        {
            { "search_text", searchText },
            { "type_id", typeId ?? 0 }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnagePart>(
            ConnectionString,
            "sp_dunnage_parts_search",
            MapFromReader,
            parameters
        );
    }

    private static Model_DunnagePart MapFromReader(IDataReader reader)
    {
        return new Model_DunnagePart
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
            SpecValues = reader.IsDBNull(reader.GetOrdinal("spec_values")) ? "{}" : reader.GetString(reader.GetOrdinal("spec_values")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
