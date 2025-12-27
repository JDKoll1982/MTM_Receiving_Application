using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Data.Dunnage;

public class Dao_DunnageSpec
{
    private readonly string _connectionString;

    public Dao_DunnageSpec(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetByTypeAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageSpec>(
            _connectionString,
            "sp_dunnage_specs_get_by_type",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageSpec>(
            _connectionString,
            "sp_dunnage_specs_get_all",
            MapFromReader
        );
    }

    public virtual async Task<Model_Dao_Result<Model_DunnageSpec>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnageSpec>(
            _connectionString,
            "sp_dunnage_specs_get_by_id",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> InsertAsync(int typeId, string specKey, string specValue, string user)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_type_id", typeId),
            new MySqlParameter("@p_spec_key", specKey),
            new MySqlParameter("@p_spec_value", specValue),
            new MySqlParameter("@p_user", user),
            pNewId
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_dunnage_specs_insert",
            parameters,
            _connectionString
        );

        if (result.IsSuccess)
        {
            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                return DaoResultFactory.Success<int>(Convert.ToInt32(pNewId.Value));
            }
            return DaoResultFactory.Failure<int>("Failed to retrieve new ID");
        }

        return DaoResultFactory.Failure<int>(result.ErrorMessage, result.Exception);
    }

    public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string specValue, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "spec_value", specValue },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_specs_update",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_specs_delete_by_id",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteByTypeAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_specs_delete_by_type",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> CountPartsUsingSpecAsync(int typeId, string specKey)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId },
            { "spec_key", specKey }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            _connectionString,
            "sp_dunnage_specs_count_parts_using_spec",
            reader => reader.GetInt32(reader.GetOrdinal("part_count")),
            parameters
        );
    }

    private Model_DunnageSpec MapFromReader(IDataReader reader)
    {
        return new Model_DunnageSpec
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
            SpecKey = reader.GetString(reader.GetOrdinal("spec_key")),
            SpecValue = reader.IsDBNull(reader.GetOrdinal("spec_value")) ? "{}" : reader.GetString(reader.GetOrdinal("spec_value")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}

