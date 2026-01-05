using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for volvo_parts_master table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_VolvoPart
{
    private readonly string _connectionString;

    public Dao_VolvoPart(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Gets all Volvo parts (optionally including inactive)
    /// </summary>
    /// <param name="includeInactive"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(bool includeInactive = false)
    {
        var parameters = new Dictionary<string, object>
        {
            { "include_inactive", includeInactive ? 1 : 0 }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_volvo_part_master_get_all",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Gets a part by part number
    /// </summary>
    /// <param name="partNumber"></param>
    public async Task<Model_Dao_Result<Model_VolvoPart>> GetByIdAsync(string partNumber)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_number", partNumber }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_volvo_part_master_get_by_id",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Inserts a new Volvo part
    /// </summary>
    /// <param name="part"></param>
    public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPart part)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_number", part.PartNumber },
            { "description", part.Description ?? (object)DBNull.Value },
            { "quantity_per_skid", part.QuantityPerSkid },
            { "is_active", part.IsActive ? 1 : 0 }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_part_master_insert",
            parameters
        );
    }

    /// <summary>
    /// Updates an existing Volvo part
    /// </summary>
    /// <param name="part"></param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_number", part.PartNumber },
            { "description", part.Description ?? (object)DBNull.Value },
            { "quantity_per_skid", part.QuantityPerSkid }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_part_master_update",
            parameters
        );
    }

    /// <summary>
    /// Deactivates a Volvo part (soft delete)
    /// </summary>
    /// <param name="partNumber"></param>
    public async Task<Model_Dao_Result> DeactivateAsync(string partNumber)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_number", partNumber },
            { "is_active", 0 }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_part_master_set_active",
            parameters
        );
    }

    private static Model_VolvoPart MapFromReader(IDataReader reader)
    {
        return new Model_VolvoPart
        {
            PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
            Description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description")),
            QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date"))
        };
    }
}
