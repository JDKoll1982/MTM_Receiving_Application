using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for routing rules CRUD operations
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_RoutingRule
{
    private readonly string _connectionString;

    public Dao_RoutingRule(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all active routing rules ordered by priority
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_RoutingRule>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_RoutingRule_GetAll",
            MapFromReader
        );
    }

    /// <summary>
    /// Retrieves a routing rule by ID
    /// </summary>
    /// <param name="id"></param>
    public async Task<Model_Dao_Result<Model_RoutingRule>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_RoutingRule_GetById",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Inserts a new routing rule with pattern matching
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="createdBy"></param>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_RoutingRule rule, int createdBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_match_type", rule.MatchType },
            { "p_pattern", rule.Pattern },
            { "p_destination_location", rule.DestinationLocation },
            { "p_priority", rule.Priority },
            { "p_created_by", createdBy }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_RoutingRule_Insert",
            reader => reader.GetInt32(reader.GetOrdinal("id")),
            parameters
        );
    }

    /// <summary>
    /// Updates an existing routing rule
    /// </summary>
    /// <param name="rule"></param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_RoutingRule rule)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", rule.Id },
            { "p_match_type", rule.MatchType },
            { "p_pattern", rule.Pattern },
            { "p_destination_location", rule.DestinationLocation },
            { "p_priority", rule.Priority }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_RoutingRule_Update",
            parameters
        );
    }

    /// <summary>
    /// Deletes a routing rule (soft delete)
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
            "sp_Settings_RoutingRule_Delete",
            parameters
        );
    }

    /// <summary>
    /// Finds the first matching routing rule for a value
    /// </summary>
    /// <param name="matchType"></param>
    /// <param name="value"></param>
    public async Task<Model_Dao_Result<Model_RoutingRule>> FindMatchAsync(string matchType, string value)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_match_type", matchType },
            { "p_value", value }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_RoutingRule_FindMatch",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Finds the first matching routing rule for a part number (spec compatibility)
    /// </summary>
    /// <param name="partNumber"></param>
    public async Task<Model_Dao_Result<Model_RoutingRule>> GetByPartNumberAsync(string partNumber)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_part_number", partNumber }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_RoutingRule_GetByPartNumber",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Maps database reader to Model_RoutingRule
    /// </summary>
    /// <param name="reader"></param>
    private static Model_RoutingRule MapFromReader(IDataReader reader)
    {
        return new Model_RoutingRule
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            MatchType = reader.GetString(reader.GetOrdinal("match_type")),
            Pattern = reader.GetString(reader.GetOrdinal("pattern")),
            DestinationLocation = reader.GetString(reader.GetOrdinal("destination_location")),
            Priority = reader.GetInt32(reader.GetOrdinal("priority")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
        };
    }
}
