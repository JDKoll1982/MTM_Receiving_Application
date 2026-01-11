using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for package types CRUD operations
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_PackageType
{
    private readonly string _connectionString;

    public Dao_PackageType(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all active package types
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_PackageType>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Dunnage_Types_GetAll",
            MapFromReader
        );
    }

    /// <summary>
    /// Retrieves a package type by ID
    /// </summary>
    /// <param name="id"></param>
    public async Task<Model_Dao_Result<Model_PackageType>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Dunnage_Types_GetById",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Inserts a new package type with validation
    /// </summary>
    /// <param name="packageType"></param>
    /// <param name="createdBy"></param>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_PackageType packageType, int createdBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_name", packageType.Name },
            { "p_code", packageType.Code },
            { "p_created_by", createdBy }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Dunnage_Types_Insert",
            reader => reader.GetInt32(reader.GetOrdinal("id")),
            parameters
        );
    }

    /// <summary>
    /// Updates an existing package type
    /// </summary>
    /// <param name="packageType"></param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_PackageType packageType)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", packageType.Id },
            { "p_name", packageType.Name },
            { "p_code", packageType.Code }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_Types_Update",
            parameters
        );
    }

    /// <summary>
    /// Deletes a package type (validates not in use)
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
            "sp_Receiving_PackageTypes_Delete",
            parameters
        );
    }

    /// <summary>
    /// Gets usage count for a package type
    /// </summary>
    /// <param name="id"></param>
    public async Task<Model_Dao_Result<int>> GetUsageCountAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Dunnage_Types_GetUsageCount",
            reader => reader.GetInt32(reader.GetOrdinal("usage_count")),
            parameters
        );
    }

    /// <summary>
    /// Maps database reader to Model_PackageType
    /// </summary>
    /// <param name="reader"></param>
    private static Model_PackageType MapFromReader(IDataReader reader)
    {
        return new Model_PackageType
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Code = reader.GetString(reader.GetOrdinal("code")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
        };
    }
}
