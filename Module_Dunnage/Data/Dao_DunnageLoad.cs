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
        var parameters = new MySqlParameter[]
        {
            new("@p_load_uuid", MySqlDbType.VarChar, 36) { Value = load.LoadUuid.ToString() },
            new("@p_part_id", MySqlDbType.VarChar, 50) { Value = load.PartId },
            new("@p_quantity", MySqlDbType.Decimal)
            {
                Value = load.Quantity,
                Precision = 10,
                Scale = 2,
            },
            new("@p_po_number", MySqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(load.PoNumber)
                    ? DBNull.Value
                    : (object)load.PoNumber,
            },
            new("@p_type_id", MySqlDbType.Int32)
            {
                Value = load.TypeId.HasValue ? (object)load.TypeId.Value : DBNull.Value,
            },
            new("@p_type_name", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.TypeName)
                    ? DBNull.Value
                    : (object)load.TypeName,
            },
            new("@p_type_icon", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.TypeIcon)
                    ? DBNull.Value
                    : (object)load.TypeIcon,
            },
            new("@p_location", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.Location)
                    ? DBNull.Value
                    : (object)load.Location,
            },
            new("@p_label_number", MySqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(load.LabelNumber)
                    ? DBNull.Value
                    : (object)load.LabelNumber,
            },
            new("@p_part_skid_sequence", MySqlDbType.Int32)
            {
                Value = load.PartSkidSequence.HasValue
                    ? (object)load.PartSkidSequence.Value
                    : DBNull.Value,
            },
            new("@p_part_skid_total", MySqlDbType.Int32)
            {
                Value = load.PartSkidTotal.HasValue
                    ? (object)load.PartSkidTotal.Value
                    : DBNull.Value,
            },
            new("@p_udc1", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc1 ?? DBNull.Value },
            new("@p_udc2", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc2 ?? DBNull.Value },
            new("@p_udc3", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc3 ?? DBNull.Value },
            new("@p_udc4", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc4 ?? DBNull.Value },
            new("@p_udc5", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc5 ?? DBNull.Value },
            new("@p_udc6", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc6 ?? DBNull.Value },
            new("@p_udc7", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc7 ?? DBNull.Value },
            new("@p_udc8", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc8 ?? DBNull.Value },
            new("@p_udc9", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc9 ?? DBNull.Value },
            new("@p_udc10", MySqlDbType.VarChar, 255) { Value = (object?)load.Udc10 ?? DBNull.Value },
            new("@p_user", MySqlDbType.VarChar, 50) { Value = user },
        };

        return await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_Loads_Update",
            parameters,
            _connectionString
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
        var hasQuantityTypeColumn = HasColumn(reader, "quantity_type");

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
            QuantityType =
                !hasQuantityTypeColumn ? "Quantity"
                : reader.IsDBNull(reader.GetOrdinal("quantity_type")) ? "Quantity"
                : reader.GetString(reader.GetOrdinal("quantity_type")),
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
            PartSkidSequence = reader.IsDBNull(reader.GetOrdinal("part_skid_sequence"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("part_skid_sequence")),
            PartSkidTotal = reader.IsDBNull(reader.GetOrdinal("part_skid_total"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("part_skid_total")),
            Udc1 = ReadUdc(reader, "udc1"),
            Udc2 = ReadUdc(reader, "udc2"),
            Udc3 = ReadUdc(reader, "udc3"),
            Udc4 = ReadUdc(reader, "udc4"),
            Udc5 = ReadUdc(reader, "udc5"),
            Udc6 = ReadUdc(reader, "udc6"),
            Udc7 = ReadUdc(reader, "udc7"),
            Udc8 = ReadUdc(reader, "udc8"),
            Udc9 = ReadUdc(reader, "udc9"),
            Udc10 = ReadUdc(reader, "udc10"),
        };
    }

    private static string? ReadUdc(IDataReader reader, string columnName)
    {
        if (!HasColumn(reader, columnName))
        {
            return null;
        }

        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static bool HasColumn(IDataReader reader, string columnName)
    {
        for (var index = 0; index < reader.FieldCount; index++)
        {
            if (
                string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }

        return false;
    }
}
