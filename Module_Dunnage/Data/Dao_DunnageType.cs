using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;


namespace MTM_Receiving_Application.Module_Dunnage.Data;

public class Dao_DunnageType
{
    private readonly string _connectionString;

    public Dao_DunnageType(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllAsync()
    {
        Console.WriteLine($"[Dao_DunnageType] GetAllAsync called");
        Console.WriteLine($"[Dao_DunnageType] Connection string: {_connectionString}");

        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageType>(
            _connectionString,
            "sp_Dunnage_Types_GetAll",
            MapFromReader
        );

        Console.WriteLine($"[Dao_DunnageType] Result - IsSuccess: {result.IsSuccess}, Data Count: {result.Data?.Count ?? 0}, Error: {result.ErrorMessage}");

        return result;
    }

    public virtual async Task<Model_Dao_Result<Model_DunnageType>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnageType>(
            _connectionString,
            "sp_Dunnage_Types_GetById",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> InsertAsync(string typeName, string icon, string user)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_type_name", typeName),
            new MySqlParameter("@p_icon", icon),
            new MySqlParameter("@p_user", user),
            pNewId
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_dunnage_types_insert",
            parameters,
            _connectionString
        );

        if (result.IsSuccess)
        {
            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
            }
            return Model_Dao_Result_Factory.Failure<int>("Failed to retrieve new ID");
        }

        return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
    }

    public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string typeName, string icon, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "type_name", typeName },
            { "icon", icon },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_types_update",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_types_delete",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> CountPartsAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            _connectionString,
            "sp_Dunnage_Types_CountParts",
            reader => reader.GetInt32(reader.GetOrdinal("part_count")),
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> CountTransactionsAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            _connectionString,
            "sp_Dunnage_Types_CountTransactions",
            reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
            parameters
        );
    }

    /// <summary>
    /// Check if a dunnage type name already exists (for duplicate detection during create/update)
    /// </summary>
    /// <param name="typeName">Type name to check</param>
    /// <param name="excludeId">ID to exclude from check (null for new types, type ID for updates)</param>
    /// <returns>True if duplicate exists, false otherwise</returns>
    public virtual async Task<Model_Dao_Result<bool>> CheckDuplicateNameAsync(string typeName, int? excludeId = null)
    {
        var pExists = new MySqlParameter("@p_exists", MySqlDbType.Bit)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_type_name", typeName),
            new MySqlParameter("@p_exclude_id", excludeId.HasValue ? (object)excludeId.Value : DBNull.Value),
            pExists
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_Types_CheckDuplicate",
            parameters,
            _connectionString
        );

        if (result.IsSuccess)
        {
            if (pExists.Value != null && pExists.Value != DBNull.Value)
            {
                return Model_Dao_Result_Factory.Success<bool>(Convert.ToBoolean(pExists.Value));
            }
            return Model_Dao_Result_Factory.Success<bool>(false);
        }

        return Model_Dao_Result_Factory.Failure<bool>(result.ErrorMessage, result.Exception);
    }

    private Model_DunnageType MapFromReader(IDataReader reader)
    {
        return new Model_DunnageType
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            TypeName = reader.GetString(reader.GetOrdinal("type_name")),
            Icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? "Help" : reader.GetString(reader.GetOrdinal("icon")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
