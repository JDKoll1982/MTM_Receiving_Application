using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MySql.Data.MySqlClient;

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

    public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate },
            { "end_date", endDate },
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
        var parameters = new Dictionary<string, object> { { "load_uuid", loadUuid.ToString() } };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnageLoad>(
            _connectionString,
            "sp_Dunnage_Loads_GetById",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> UpdateAsync(Model_DunnageLoad load, string user)
    {
        var parameters = new Dictionary<string, object>
        {
            { "load_uuid", load.LoadUuid.ToString() },
            { "part_id", load.PartId },
            { "quantity", load.Quantity },
            {
                "po_number",
                string.IsNullOrWhiteSpace(load.PoNumber) ? DBNull.Value : (object)load.PoNumber
            },
            { "type_id", load.TypeId.HasValue ? load.TypeId.Value : DBNull.Value },
            {
                "type_name",
                string.IsNullOrWhiteSpace(load.TypeName) ? DBNull.Value : (object)load.TypeName
            },
            {
                "type_icon",
                string.IsNullOrWhiteSpace(load.TypeIcon) ? DBNull.Value : (object)load.TypeIcon
            },
            {
                "location",
                string.IsNullOrWhiteSpace(load.Location) ? DBNull.Value : (object)load.Location
            },
            {
                "label_number",
                string.IsNullOrWhiteSpace(load.LabelNumber)
                    ? DBNull.Value
                    : (object)load.LabelNumber
            },
            { "specs_json", SerializeSpecValues(load) is { } specsJson ? specsJson : DBNull.Value },
            { "user", user },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_Loads_Update",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteAsync(Guid loadUuid)
    {
        var parameters = new Dictionary<string, object> { { "load_uuid", loadUuid.ToString() } };

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
            TypeId = reader.IsDBNull(reader.GetOrdinal("type_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("type_id")),
            TypeName = reader.IsDBNull(reader.GetOrdinal("type_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("type_name")),
            DunnageType = reader.IsDBNull(reader.GetOrdinal("type_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("type_name")),
            TypeIcon = reader.IsDBNull(reader.GetOrdinal("type_icon"))
                ? "Help"
                : reader.GetString(reader.GetOrdinal("type_icon")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            PoNumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("po_number")),
            ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by"))
                ? null
                : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("modified_date")),
            Location = reader.IsDBNull(reader.GetOrdinal("location"))
                ? null
                : reader.GetString(reader.GetOrdinal("location")),
            LabelNumber = reader.IsDBNull(reader.GetOrdinal("label_number"))
                ? null
                : reader.GetString(reader.GetOrdinal("label_number")),
            SpecValues = DeserializeSpecValues(reader),
        };
    }

    private static string? SerializeSpecValues(Model_DunnageLoad load)
    {
        var specs = load.SpecValues ?? load.Specs;
        if (specs == null || specs.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(specs);
    }

    private static Dictionary<string, object>? DeserializeSpecValues(IDataReader reader)
    {
        var ordinal = reader.GetOrdinal("specs_json");
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        var json = reader.GetString(ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
