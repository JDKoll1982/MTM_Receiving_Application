using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data;

/// <summary>
/// MySQL data access for the lot_markings and lot_marking_part_scope tables.
/// Every call goes through a stored procedure; there is no raw SQL here.
/// </summary>
public class Dao_Tool_LotMarking
{
    private readonly string _connectionString;

    public Dao_Tool_LotMarking(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>Lists every recorded marking.</summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetAllMarkingsAsync() =>
        await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Tool_LotMarking>(
            _connectionString,
            "sp_LotMarkings_GetAll",
            MapMarking
        );

    /// <summary>Finds markings by heat number.</summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> SearchMarkingsByHeatAsync(
        string? searchText
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_search_text", searchText ?? string.Empty },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Tool_LotMarking>(
            _connectionString,
            "sp_LotMarkings_GetByHeat",
            MapMarking,
            parameters
        );
    }

    /// <summary>Reads the marking already recorded for one lot, if any.</summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> ResolveAsync(
        string heatNumberNormalized,
        string supplierId
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_heat_number_normalized", heatNumberNormalized },
            { "p_supplier_id", supplierId },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Tool_LotMarking>(
            _connectionString,
            "sp_LotMarkings_Resolve",
            MapMarking,
            parameters
        );
    }

    /// <summary>Records a marking for a lot that has never been seen. Returns the new row id.</summary>
    public virtual async Task<Model_Dao_Result<int>> InsertAsync(
        string heatNumber,
        string heatNumberNormalized,
        string supplierId,
        string? supplierName,
        string shapeGlyph,
        string colorCode,
        string markingSource,
        string? firstSeenPartNumber,
        string? resolvedBy
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_heat_number", heatNumber },
            { "p_heat_number_normalized", heatNumberNormalized },
            { "p_supplier_id", supplierId },
            { "p_supplier_name", supplierName ?? string.Empty },
            { "p_shape_code", shapeGlyph },
            { "p_color_code", colorCode },
            { "p_marking_source", markingSource },
            { "p_first_seen_part_number", firstSeenPartNumber ?? string.Empty },
            { "p_resolved_by", resolvedBy ?? string.Empty },
        };

        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<int>(
            _connectionString,
            "sp_LotMarkings_Insert",
            reader => reader.GetInt32(reader.GetOrdinal("new_id")),
            parameters
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        return Model_Dao_Result_Factory.Success<int>(
            result.Data is { Count: > 0 } ? result.Data[0] : 0
        );
    }

    /// <summary>Replaces a marking by hand. Returns the number of rows changed.</summary>
    public virtual async Task<Model_Dao_Result<int>> UpdateMarkingAsync(
        int id,
        string shapeGlyph,
        string colorCode,
        string markingSource,
        string reason,
        string actingUser
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_shape_code", shapeGlyph },
            { "p_color_code", colorCode },
            { "p_marking_source", markingSource },
            { "p_change_reason", reason },
            { "p_resolved_by", actingUser },
        };

        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<int>(
            _connectionString,
            "sp_LotMarkings_UpdateMarking",
            reader => reader.GetInt32(reader.GetOrdinal("rows_updated")),
            parameters
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        return Model_Dao_Result_Factory.Success<int>(
            result.Data is { Count: > 0 } ? result.Data[0] : 0
        );
    }

    /// <summary>Locks or unlocks a marking. Returns the number of rows changed.</summary>
    public virtual async Task<Model_Dao_Result<int>> SetLockedAsync(
        int id,
        bool isLocked,
        string actingUser
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_is_locked", isLocked ? 1 : 0 },
            { "p_resolved_by", actingUser },
        };

        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<int>(
            _connectionString,
            "sp_LotMarkings_SetLocked",
            reader => reader.GetInt32(reader.GetOrdinal("rows_updated")),
            parameters
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        return Model_Dao_Result_Factory.Success<int>(
            result.Data is { Count: > 0 } ? result.Data[0] : 0
        );
    }

    /// <summary>Lists the enabled-part rows.</summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> GetEnabledPartsAsync() =>
        await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Tool_LotMarkingScope>(
            _connectionString,
            "sp_LotMarkingPartScope_GetAll",
            MapScope
        );

    /// <summary>Finds enabled-part rows by part number.</summary>
    public virtual async Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> SearchPartsAsync(
        string? searchText
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_search_text", searchText ?? string.Empty },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Tool_LotMarkingScope>(
            _connectionString,
            "sp_LotMarkingPartScope_Search",
            MapScope,
            parameters
        );
    }

    /// <summary>Adds a part to the enabled list, or re-enables a removed one.</summary>
    public virtual async Task<Model_Dao_Result<int>> UpsertPartAsync(string partId, string? addedBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_id", partId },
            { "p_added_by", addedBy ?? string.Empty },
        };

        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<int>(
            _connectionString,
            "sp_LotMarkingPartScope_Upsert",
            reader => reader.GetInt32(reader.GetOrdinal("part_scope_id")),
            parameters
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        return Model_Dao_Result_Factory.Success<int>(
            result.Data is { Count: > 0 } ? result.Data[0] : 0
        );
    }

    /// <summary>Enables or disables a part on the list.</summary>
    public virtual async Task<Model_Dao_Result> SetPartEnabledAsync(int id, bool isEnabled)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_is_enabled", isEnabled ? 1 : 0 },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_LotMarkingPartScope_SetEnabled",
            parameters
        );
    }

    /// <summary>Deletes a list row outright.</summary>
    public virtual async Task<Model_Dao_Result> DeletePartAsync(int id)
    {
        var parameters = new Dictionary<string, object> { { "p_id", id } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_LotMarkingPartScope_Delete",
            parameters
        );
    }

    /// <summary>Writes the marking codes onto an existing receiving queue row.</summary>
    public virtual async Task<Model_Dao_Result> SetQueueRowMarkingAsync(
        string loadId,
        string? shapeGlyph,
        string? colorCode
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_load_id", loadId },
            { "p_heat_shape_code", shapeGlyph ?? string.Empty },
            { "p_heat_color_code", colorCode ?? string.Empty },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Receiving_LabelData_SetMarking",
            parameters
        );
    }

    private static Model_Tool_LotMarking MapMarking(IDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            HeatNumber = ReadString(reader, "heat_number"),
            HeatNumberNormalized = ReadString(reader, "heat_number_normalized"),
            SupplierId = ReadString(reader, "supplier_id"),
            SupplierName = ReadString(reader, "supplier_name"),
            ShapeCode = ReadString(reader, "shape_code"),
            ColorCode = ReadString(reader, "color_code"),
            MarkingSource = ReadString(reader, "marking_source"),
            IsLocked = ReadBool(reader, "is_locked"),
            FirstSeenPartNumber = ReadString(reader, "first_seen_part_number"),
            ChangeReason = ReadString(reader, "change_reason"),
            ResolvedBy = ReadString(reader, "resolved_by"),
            ResolvedAt = ReadNullableDateTime(reader, "resolved_at"),
            Notes = ReadString(reader, "notes"),
        };

    private static Model_Tool_LotMarkingScope MapScope(IDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartId = ReadString(reader, "part_id"),
            IsEnabled = ReadBool(reader, "is_enabled"),
            AddedBy = ReadString(reader, "added_by"),
            AddedAt = ReadNullableDateTime(reader, "added_at"),
        };

    private static string ReadString(IDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetValue(ordinal).ToString() ?? string.Empty;
    }

    private static bool ReadBool(IDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return !reader.IsDBNull(ordinal) && Convert.ToInt32(reader.GetValue(ordinal)) != 0;
    }

    private static DateTime? ReadNullableDateTime(IDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
