using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data;

/// <summary>
/// MySQL data access for the settings_weldedcoils table (Welded Coils tool).
/// All writes go through the sp_Settings_WeldedCoils_* stored procedures; no raw SQL here.
/// </summary>
public class Dao_Tool_WeldedCoil
{
    private readonly string _connectionString;

    public Dao_Tool_WeldedCoil(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>Returns all welded-coil rows (active first, then by part number).</summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_WeldedCoil>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Tool_WeldedCoil>(
            _connectionString,
            "sp_Settings_WeldedCoils_GetAll",
            MapFromReader
        );
    }

    /// <summary>Adds a part (defaults isActive = 1). Returns the new row id.</summary>
    public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_partid", partId),
            pNewId,
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Settings_WeldedCoils_Insert",
            parameters,
            _connectionString
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        if (pNewId.Value != null && pNewId.Value != DBNull.Value)
        {
            return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
        }

        return Model_Dao_Result_Factory.Failure<int>("Failed to retrieve new ID");
    }

    /// <summary>Renames the part number on an existing row.</summary>
    public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string partId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_partid", partId },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_WeldedCoils_Update",
            parameters
        );
    }

    /// <summary>Flips a row between Active (true) and Inactive (false).</summary>
    public virtual async Task<Model_Dao_Result> SetActiveAsync(int id, bool isActive)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_is_active", isActive },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_WeldedCoils_SetActive",
            parameters
        );
    }

    /// <summary>Removes a welded-coil row.</summary>
    public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object> { { "p_id", id } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_WeldedCoils_Delete",
            parameters
        );
    }

    private static Model_Tool_WeldedCoil MapFromReader(IDataReader reader)
    {
        return new Model_Tool_WeldedCoil
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartId = reader.GetString(reader.GetOrdinal("partid")),
            IsActive = Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("isActive"))),
        };
    }
}
