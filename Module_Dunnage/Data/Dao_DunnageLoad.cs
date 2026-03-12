using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

public class Dao_DunnageLoad
{
    private readonly string _connectionString;

    public Dao_DunnageLoad(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageLoad>(
            _connectionString,
            "sp_Dunnage_Loads_GetAll",
            MapFromReader
        );
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate },
            { "end_date", endDate }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageLoad>(
            _connectionString,
            "sp_Dunnage_Loads_GetByDateRange",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<Model_DunnageLoad>> GetByIdAsync(Guid loadUuid)
    {
        var parameters = new Dictionary<string, object>
        {
            { "load_uuid", loadUuid.ToString() }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnageLoad>(
            _connectionString,
            "sp_Dunnage_Loads_GetById",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> UpdateAsync(Guid loadUuid, decimal quantity, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "load_uuid", loadUuid.ToString() },
            { "quantity", quantity },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_Loads_Update",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteAsync(Guid loadUuid)
    {
        var parameters = new Dictionary<string, object>
        {
            { "load_uuid", loadUuid.ToString() }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_Loads_Delete",
            parameters
        );
    }

    private Model_DunnageLoad MapFromReader(IDataReader reader)
    {
        return new Model_DunnageLoad
        {
            LoadUuid = (Guid)reader[reader.GetOrdinal("load_uuid")],
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            TypeId = reader.IsDBNull(reader.GetOrdinal("type_id")) ? null : reader.GetInt32(reader.GetOrdinal("type_id")),
            TypeName = reader.IsDBNull(reader.GetOrdinal("type_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("type_name")),
            DunnageType = reader.IsDBNull(reader.GetOrdinal("type_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("type_name")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
