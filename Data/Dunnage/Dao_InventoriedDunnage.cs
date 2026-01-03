using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;

namespace MTM_Receiving_Application.Data.Dunnage;

public class Dao_InventoriedDunnage
{
    private readonly string _connectionString;

    public Dao_InventoriedDunnage(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_InventoriedDunnage>(
            _connectionString,
            "sp_inventoried_dunnage_get_all",
            MapFromReader
        );
    }

    public virtual async Task<Model_Dao_Result<bool>> CheckAsync(string partId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_id", partId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<bool>(
            _connectionString,
            "sp_inventoried_dunnage_check",
            reader => reader.GetBoolean(reader.GetOrdinal("requires_inventory")),
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetByPartAsync(string partId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_id", partId }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_InventoriedDunnage>(
            _connectionString,
            "sp_inventoried_dunnage_get_by_part",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId, string inventoryMethod, string notes, string user)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_part_id", partId),
            new MySqlParameter("@p_inventory_method", inventoryMethod),
            new MySqlParameter("@p_notes", notes),
            new MySqlParameter("@p_user", user),
            pNewId
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_inventoried_dunnage_insert",
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

    public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string inventoryMethod, string notes, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "inventory_method", inventoryMethod },
            { "notes", notes },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_inventoried_dunnage_update",
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
            "sp_inventoried_dunnage_delete",
            parameters
        );
    }

    private Model_InventoriedDunnage MapFromReader(IDataReader reader)
    {
        return new Model_InventoriedDunnage
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            InventoryMethod = reader.IsDBNull(reader.GetOrdinal("inventory_method")) ? null : reader.GetString(reader.GetOrdinal("inventory_method")),
            Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}

