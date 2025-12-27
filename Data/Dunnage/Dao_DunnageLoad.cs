using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using System.Text.Json;

namespace MTM_Receiving_Application.Data.Dunnage;

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
            "sp_dunnage_loads_get_all",
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
            "sp_dunnage_loads_get_by_date_range",
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
            "sp_dunnage_loads_get_by_id",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> InsertAsync(Guid loadUuid, string partId, decimal quantity, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "load_uuid", loadUuid.ToString() },
            { "part_id", partId },
            { "quantity", quantity },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_loads_insert",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> InsertBatchAsync(List<Model_DunnageLoad> loads, string user)
    {
        // Serialize loads to JSON for batch insert
        // We need to map the model properties to the JSON structure expected by the SP
        var loadData = new List<object>();
        foreach (var load in loads)
        {
            loadData.Add(new
            {
                load_uuid = load.LoadUuid.ToString(),
                part_id = load.PartId,
                quantity = load.Quantity
            });
        }

        string json = JsonSerializer.Serialize(loadData);

        var parameters = new Dictionary<string, object>
        {
            { "load_data", json },
            { "user", user }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_loads_insert_batch",
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
            "sp_dunnage_loads_update",
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
            "sp_dunnage_loads_delete",
            parameters
        );
    }

    private Model_DunnageLoad MapFromReader(IDataReader reader)
    {
        return new Model_DunnageLoad
        {
            LoadUuid = (Guid)reader[reader.GetOrdinal("load_uuid")],
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
