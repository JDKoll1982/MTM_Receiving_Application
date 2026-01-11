using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for package type mappings (part prefix → package type)
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_PackageTypeMappings
{
    private readonly string _connectionString;

    public Dao_PackageTypeMappings(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all active package type mappings
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_PackageTypeMapping>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_PackageTypeMappings_GetAll",
            MapFromReader
        );
    }

    /// <summary>
    /// Gets package type for a part prefix (with default fallback)
    /// </summary>
    /// <param name="partPrefix"></param>
    public async Task<Model_Dao_Result<string>> GetByPrefixAsync(string partPrefix)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_prefix", partPrefix }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_PackageTypeMappings_GetByPrefix",
            reader => reader.IsDBNull(reader.GetOrdinal("package_type"))
                ? "Skids"
                : reader.GetString(reader.GetOrdinal("package_type")),
            parameters
        );
    }

    /// <summary>
    /// Inserts a new package type mapping
    /// </summary>
    /// <param name="mapping"></param>
    /// <param name="createdBy"></param>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_PackageTypeMapping mapping, int createdBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_prefix", mapping.PartPrefix },
            { "p_package_type", mapping.PackageType },
            { "p_is_default", mapping.IsDefault },
            { "p_display_order", mapping.DisplayOrder },
            { "p_created_by", createdBy }
        };

        var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_PackageTypeMappings_Insert",
            reader => reader.GetInt32(reader.GetOrdinal("id")),
            parameters
        );

        return result;
    }

    /// <summary>
    /// Updates an existing package type mapping
    /// </summary>
    /// <param name="mapping"></param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_PackageTypeMapping mapping)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", mapping.Id },
            { "p_package_type", mapping.PackageType },
            { "p_is_default", mapping.IsDefault },
            { "p_display_order", mapping.DisplayOrder }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_PackageTypeMappings_Update",
            parameters
        );
    }

    /// <summary>
    /// Deletes a package type mapping (soft delete)
    /// </summary>
    /// <param name="id"></param>
    public async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_PackageTypeMappings_Delete",
            parameters
        );
    }

    /// <summary>
    /// Maps database reader to Model_PackageTypeMapping
    /// </summary>
    /// <param name="reader"></param>
    private static Model_PackageTypeMapping MapFromReader(IDataReader reader)
    {
        return new Model_PackageTypeMapping
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartPrefix = reader.GetString(reader.GetOrdinal("part_prefix")),
            PackageType = reader.GetString(reader.GetOrdinal("package_type")),
            IsDefault = reader.GetBoolean(reader.GetOrdinal("is_default")),
            DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
        };
    }
}
