using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for receiving_quality_holds table
/// Provides CRUD operations for quality hold tracking using stored procedures
/// </summary>
public class Dao_Receiving_Repository_QualityHold
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_QualityHold(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a new quality hold record into the database
    /// </summary>
    /// <param name="qualityHold">QualityHold model to insert</param>
    /// <returns>Model_Dao_Result with success status and quality_hold_id</returns>
    public async Task<Model_Dao_Result<int>> InsertQualityHoldAsync(Model_Receiving_Entity_QualityHold qualityHold)
    {
        try
        {
            // Prepare stored procedure parameters
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_LoadID", qualityHold.LoadID),
                new MySqlParameter("@p_PartID", qualityHold.PartID ?? string.Empty),
                new MySqlParameter("@p_RestrictionType", qualityHold.RestrictionType ?? string.Empty),
                new MySqlParameter("@p_QualityAcknowledgedBy", (object?)qualityHold.QualityAcknowledgedBy ?? DBNull.Value),
                new MySqlParameter("@p_QualityAcknowledgedAt", (object?)qualityHold.QualityAcknowledgedAt ?? DBNull.Value),
                new MySqlParameter("@p_QualityHoldID", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            // Execute stored procedure
            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Receiving_QualityHolds_Insert",
                parameters,
                _connectionString
            );

            if (!result.Success)
            {
                return new Model_Dao_Result<int>
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    Severity = result.Severity
                };
            }

            // Extract output values
            var qualityHoldId = Convert.ToInt32(parameters[5].Value);

            return new Model_Dao_Result<int>
            {
                Success = true,
                Data = qualityHoldId
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<int>
            {
                Success = false,
                ErrorMessage = $"Unexpected error inserting quality hold: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Retrieves all quality hold records for a specific load
    /// </summary>
    /// <param name="loadId">Load ID to query</param>
    /// <returns>Model_Dao_Result with list of quality holds</returns>
    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>> GetQualityHoldsByLoadIDAsync(int loadId)
    {
        try
        {
            if (loadId <= 0)
            {
                return new Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>
                {
                    Success = false,
                    ErrorMessage = "LoadID must be positive",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var parameters = new Dictionary<string, object>
            {
                { "@p_LoadID", loadId }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_QualityHolds_GetByLoadID",
                reader => new Model_Receiving_Entity_QualityHold
                {
                    QualityHoldID = reader.GetInt32(reader.GetOrdinal("quality_hold_id")),
                    LoadID = reader.GetInt32(reader.GetOrdinal("load_id")),
                    PartID = reader.IsDBNull(reader.GetOrdinal("part_id")) ? string.Empty : reader.GetString(reader.GetOrdinal("part_id")),
                    RestrictionType = reader.IsDBNull(reader.GetOrdinal("restriction_type")) ? string.Empty : reader.GetString(reader.GetOrdinal("restriction_type")),
                    QualityAcknowledgedBy = reader.IsDBNull(reader.GetOrdinal("quality_acknowledged_by")) ? null : reader.GetString(reader.GetOrdinal("quality_acknowledged_by")),
                    QualityAcknowledgedAt = reader.IsDBNull(reader.GetOrdinal("quality_acknowledged_at")) ? null : reader.GetDateTime(reader.GetOrdinal("quality_acknowledged_at")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime(reader.GetOrdinal("updated_at"))
                },
                parameters
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>
            {
                Success = false,
                ErrorMessage = $"Unexpected error retrieving quality holds: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Updates quality hold acknowledgment information
    /// </summary>
    /// <param name="qualityHoldId">Quality hold ID to update</param>
    /// <param name="acknowledgedBy">Username who acknowledged</param>
    /// <param name="acknowledgedAt">Timestamp of acknowledgment</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> UpdateQualityHoldAcknowledgmentAsync(int qualityHoldId, string acknowledgedBy, DateTime acknowledgedAt)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_QualityHoldID", qualityHoldId),
                new MySqlParameter("@p_QualityAcknowledgedBy", acknowledgedBy ?? string.Empty),
                new MySqlParameter("@p_QualityAcknowledgedAt", acknowledgedAt),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Receiving_QualityHolds_Update",
                parameters,
                _connectionString
            );

            if (!result.Success)
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    Severity = result.Severity
                };
            }

            return new Model_Dao_Result
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error updating quality hold: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }
}
