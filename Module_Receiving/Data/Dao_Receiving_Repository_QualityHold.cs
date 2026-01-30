using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for quality hold records
/// Manages CRUD operations for tbl_Receiving_QualityHold
/// </summary>
public class Dao_Receiving_Repository_QualityHold
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_QualityHold(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Inserts a new quality hold record with initial acknowledgment
    /// </summary>
    /// <param name="qualityHold">Quality hold entity to insert</param>
    /// <returns>Result with QualityHoldId (GUID string)</returns>
    public async Task<Model_Dao_Result<string>> InsertQualityHoldAsync(Model_Receiving_TableEntitys_QualityHold qualityHold)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(qualityHold);

            var qualityHoldId = string.IsNullOrWhiteSpace(qualityHold.QualityHoldId) 
                ? Guid.NewGuid().ToString() 
                : qualityHold.QualityHoldId;

            var parameters = new Dictionary<string, object>
            {
                { "p_QualityHoldId", qualityHoldId },
                { "p_LineId", qualityHold.LineId },
                { "p_TransactionId", qualityHold.TransactionId },
                { "p_PartNumber", qualityHold.PartNumber },
                { "p_PartPattern", qualityHold.PartPattern },
                { "p_RestrictionType", qualityHold.RestrictionType },
                { "p_LoadNumber", qualityHold.LoadNumber },
                { "p_TotalWeight", (object?)qualityHold.TotalWeight ?? DBNull.Value },
                { "p_PackageType", (object?)qualityHold.PackageType ?? DBNull.Value },
                { "p_UserAcknowledgedDate", (object?)qualityHold.UserAcknowledgedDate ?? DBNull.Value },
                { "p_UserAcknowledgedBy", (object?)qualityHold.UserAcknowledgedBy ?? DBNull.Value },
                { "p_UserAcknowledgmentMessage", (object?)qualityHold.UserAcknowledgmentMessage ?? DBNull.Value },
                { "p_Notes", (object?)qualityHold.Notes ?? DBNull.Value },
                { "p_CreatedBy", qualityHold.CreatedBy }
            };

            _logger.LogInfo($"Inserting quality hold: LineId={qualityHold.LineId}, PartNumber={qualityHold.PartNumber}, Pattern={qualityHold.PartPattern}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_QualityHold_Insert",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Quality hold inserted successfully: {qualityHoldId}");
                return new Model_Dao_Result<string>
                {
                    Success = true,
                    Data = qualityHoldId,
                    AffectedRows = result.AffectedRows,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            _logger.LogError($"Failed to insert quality hold: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<string>(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in InsertQualityHoldAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Error inserting quality hold: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates quality hold with final acknowledgment (step 2 of 2)
    /// </summary>
    /// <param name="qualityHoldId">Quality hold ID to update</param>
    /// <param name="finalAcknowledgedDate">Date of final acknowledgment</param>
    /// <param name="finalAcknowledgedBy">Username of person acknowledging</param>
    /// <param name="finalAcknowledgmentMessage">Message displayed during acknowledgment</param>
    /// <param name="modifiedBy">Username performing the update</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> UpdateFinalAcknowledgmentAsync(
        string qualityHoldId,
        DateTime finalAcknowledgedDate,
        string finalAcknowledgedBy,
        string? finalAcknowledgmentMessage,
        string modifiedBy)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(qualityHoldId);
            ArgumentException.ThrowIfNullOrWhiteSpace(finalAcknowledgedBy);
            ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy);

            var parameters = new Dictionary<string, object>
            {
                { "p_QualityHoldId", qualityHoldId },
                { "p_FinalAcknowledgedDate", finalAcknowledgedDate },
                { "p_FinalAcknowledgedBy", finalAcknowledgedBy },
                { "p_FinalAcknowledgmentMessage", (object?)finalAcknowledgmentMessage ?? DBNull.Value },
                { "p_ModifiedBy", modifiedBy }
            };

            _logger.LogInfo($"Updating final acknowledgment for quality hold: {qualityHoldId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_QualityHold_UpdateFinalAcknowledgment",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Final acknowledgment updated successfully for quality hold: {qualityHoldId}");
                return new Model_Dao_Result
                {
                    Success = true,
                    AffectedRows = result.AffectedRows,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            _logger.LogError($"Failed to update final acknowledgment: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpdateFinalAcknowledgmentAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating final acknowledgment: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all quality hold records for a specific line
    /// </summary>
    /// <param name="lineId">Line ID to query</param>
    /// <returns>Result with list of quality hold records</returns>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_QualityHold>>> GetQualityHoldsByLineIdAsync(string lineId)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(lineId);

            var parameters = new Dictionary<string, object>
            {
                { "p_LineId", lineId }
            };

            _logger.LogInfo($"Retrieving quality holds for LineId: {lineId}");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_QualityHold_SelectByLineID",
                MapQualityHoldFromDataReader,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} quality hold records for LineId: {lineId}");
                return new Model_Dao_Result<List<Model_Receiving_TableEntitys_QualityHold>>
                {
                    Success = true,
                    Data = result.Data ?? new List<Model_Receiving_TableEntitys_QualityHold>(),
                    AffectedRows = result.AffectedRows,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            _logger.LogError($"Failed to retrieve quality holds: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_QualityHold>>(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetQualityHoldsByLineIdAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_QualityHold>>($"Error retrieving quality holds: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps a DataReader row to a QualityHold entity
    /// </summary>
    private Model_Receiving_TableEntitys_QualityHold MapQualityHoldFromDataReader(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_QualityHold
        {
            QualityHoldId = reader["QualityHoldId"].ToString() ?? string.Empty,
            LineId = reader["LineId"].ToString() ?? string.Empty,
            TransactionId = reader["TransactionId"].ToString() ?? string.Empty,
            PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
            PartPattern = reader["PartPattern"].ToString() ?? string.Empty,
            RestrictionType = reader["RestrictionType"].ToString() ?? string.Empty,
            UserAcknowledgedDate = reader["UserAcknowledgedDate"] == DBNull.Value ? null : (DateTime?)reader["UserAcknowledgedDate"],
            UserAcknowledgedBy = reader["UserAcknowledgedBy"] == DBNull.Value ? null : reader["UserAcknowledgedBy"].ToString(),
            UserAcknowledgmentMessage = reader["UserAcknowledgmentMessage"] == DBNull.Value ? null : reader["UserAcknowledgmentMessage"].ToString(),
            FinalAcknowledgedDate = reader["FinalAcknowledgedDate"] == DBNull.Value ? null : (DateTime?)reader["FinalAcknowledgedDate"],
            FinalAcknowledgedBy = reader["FinalAcknowledgedBy"] == DBNull.Value ? null : reader["FinalAcknowledgedBy"].ToString(),
            FinalAcknowledgmentMessage = reader["FinalAcknowledgmentMessage"] == DBNull.Value ? null : reader["FinalAcknowledgmentMessage"].ToString(),
            IsFullyAcknowledged = reader["IsFullyAcknowledged"] != DBNull.Value && Convert.ToBoolean(reader["IsFullyAcknowledged"]),
            QualityInspectorName = reader["QualityInspectorName"] == DBNull.Value ? null : reader["QualityInspectorName"].ToString(),
            QualityInspectorDate = reader["QualityInspectorDate"] == DBNull.Value ? null : (DateTime?)reader["QualityInspectorDate"],
            QualityInspectorNotes = reader["QualityInspectorNotes"] == DBNull.Value ? null : reader["QualityInspectorNotes"].ToString(),
            IsReleased = reader["IsReleased"] != DBNull.Value && Convert.ToBoolean(reader["IsReleased"]),
            ReleasedDate = reader["ReleasedDate"] == DBNull.Value ? null : (DateTime?)reader["ReleasedDate"],
            LoadNumber = Convert.ToInt32(reader["LoadNumber"]),
            TotalWeight = reader["TotalWeight"] == DBNull.Value ? null : Convert.ToDecimal(reader["TotalWeight"]),
            PackageType = reader["PackageType"] == DBNull.Value ? null : reader["PackageType"].ToString(),
            Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
            IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
            IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
            CreatedBy = reader["CreatedBy"].ToString() ?? string.Empty,
            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
            ModifiedBy = reader["ModifiedBy"] == DBNull.Value ? null : reader["ModifiedBy"].ToString(),
            ModifiedDate = reader["ModifiedDate"] == DBNull.Value ? null : (DateTime?)reader["ModifiedDate"]
        };
    }
}
