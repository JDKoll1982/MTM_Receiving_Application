using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Data;

/// <summary>
/// MySQL data access for the welded part numbers list on the Receiving settings page.
/// </summary>
/// <remarks>
/// Shares the <c>settings_weldedcoils</c> table with the Extra Tools Welded Coils page, so it uses
/// the same <c>sp_Settings_WeldedCoils_*</c> stored procedures. All writes go through them; there is
/// no raw MySQL here.
/// </remarks>
public class Dao_Settings_ReceivingWeldedParts
{
    private readonly string _connectionString;

    public Dao_Settings_ReceivingWeldedParts(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>Returns every welded part number, ordered by part.</summary>
    public virtual async Task<
        Model_Dao_Result<List<Model_Settings_ReceivingWeldedPart>>
    > GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Settings_ReceivingWeldedPart>(
            _connectionString,
            "sp_Settings_WeldedCoils_GetAll",
            MapFromReader
        );
    }

    /// <summary>Adds a part number. Returns the new row id.</summary>
    public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId)
    {
        var newId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_partid", partId),
            newId,
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

        if (newId.Value is not null and not System.DBNull)
        {
            return Model_Dao_Result_Factory.Success<int>(System.Convert.ToInt32(newId.Value));
        }

        return Model_Dao_Result_Factory.Failure<int>("Failed to retrieve the new part id.");
    }

    /// <summary>Removes a part number.</summary>
    public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object> { { "p_id", id } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_WeldedCoils_Delete",
            parameters
        );
    }

    private static Model_Settings_ReceivingWeldedPart MapFromReader(IDataReader reader)
    {
        return new Model_Settings_ReceivingWeldedPart
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartId = reader.GetString(reader.GetOrdinal("partid")),
        };
    }
}
