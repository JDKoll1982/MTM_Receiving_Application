using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Data.Dunnage;

public static class Dao_DunnageType
{
    private static string ConnectionString => Helper_Database_Variables.GetConnectionString();

    public static async Task<DaoResult<List<Model_DunnageType>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageType>(
            ConnectionString,
            "sp_dunnage_types_get_all",
            MapFromReader
        );
    }

    public static async Task<DaoResult<Model_DunnageType>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnageType>(
            ConnectionString,
            "sp_dunnage_types_get_by_id",
            MapFromReader,
            parameters
        );
    }

    public static async Task<DaoResult<int>> InsertAsync(string typeName, string user)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_type_name", typeName),
            new MySqlParameter("@p_user", user),
            pNewId
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_dunnage_types_insert",
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

    public static async Task<Model_Dao_Result> UpdateAsync(int id, string typeName, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "type_name", typeName },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            ConnectionString,
            "sp_dunnage_types_update",
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
            "sp_dunnage_types_delete",
            parameters
        );
    }

    public static async Task<DaoResult<int>> CountPartsAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            ConnectionString,
            "sp_dunnage_types_count_parts",
            reader => reader.GetInt32(reader.GetOrdinal("part_count")),
            parameters
        );
    }

    public static async Task<DaoResult<int>> CountTransactionsAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            ConnectionString,
            "sp_dunnage_types_count_transactions",
            reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
            parameters
        );
    }

    private static Model_DunnageType MapFromReader(IDataReader reader)
    {
        return new Model_DunnageType
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            TypeName = reader.GetString(reader.GetOrdinal("type_name")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
