using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;

using System.Text.Json;

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
            "sp_Dunnage_Loads_Insert",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> InsertBatchAsync(List<Model_DunnageLoad> loads, string user)
    {
        // Validate that all part IDs exist before attempting insert
        var uniquePartIds = loads.Select(l => l.PartId).Distinct().ToList();
        var daoPart = new Dao_DunnagePart(_connectionString);
        var invalidParts = new List<string>();

        foreach (var partId in uniquePartIds)
        {
            var partResult = await daoPart.GetByIdAsync(partId);
            if (!partResult.IsSuccess || partResult.Data == null)
            {
                invalidParts.Add(partId);
            }
        }

        if (invalidParts.Count > 0)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Cannot save loads: The following Part ID(s) have not been registered in the system: {string.Join(", ", invalidParts)}. " +
                              $"Please go to Admin > Manage Parts to register these parts before receiving them.",
                Severity = Enum_ErrorSeverity.Error
            };
        }

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

        var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_Loads_InsertBatch",
            parameters
        );

        // Enhance foreign key constraint error messages
        if (!result.Success && result.ErrorMessage != null)
        {
            if (result.ErrorMessage.Contains("FK_dunnage_history_part_id") ||
                result.ErrorMessage.Contains("foreign key constraint"))
            {
                result.ErrorMessage = "Cannot save loads: One or more Part IDs are not registered in the system. " +
                                     "Please go to Admin > Manage Parts to register all parts before receiving them.";
                result.Severity = Enum_ErrorSeverity.Error;
            }
        }

        return result;
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
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
