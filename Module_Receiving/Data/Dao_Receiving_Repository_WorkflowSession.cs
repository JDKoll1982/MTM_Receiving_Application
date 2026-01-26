using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for workflow sessions
/// Manages session state persistence for Guided Mode (Wizard)
/// </summary>
public class Dao_Receiving_Repository_WorkflowSession
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_WorkflowSession(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new workflow session
    /// </summary>
    /// <param name="session">Session entity to insert</param>
    /// <returns>Result with SessionId (GUID string)</returns>
    public async Task<Model_Dao_Result<string>> InsertSessionAsync(Model_Receiving_TableEntitys_WorkflowSession session)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(session);

            var sessionId = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, object>
            {
                { "p_SessionId", sessionId },
                { "p_WorkflowMode", session.WorkflowMode.ToString() },
                { "p_PONumber", (object?)session.PONumber ?? DBNull.Value },
                { "p_PartNumber", (object?)session.PartId ?? DBNull.Value },
                { "p_LoadCount", session.LoadCount },
                { "p_IsNonPO", session.IsNonPO },
                { "p_CreatedBy", $"User_{session.UserId}" }
            };

            _logger.LogInfo($"Inserting workflow session: UserId={session.UserId}, Mode={session.WorkflowMode}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_WorkflowSession_Insert",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Workflow session inserted successfully: {sessionId}");
                return new Model_Dao_Result<string>
                {
                    Success = true,
                    Data = sessionId,
                    AffectedRows = result.AffectedRows,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            _logger.LogError($"Failed to insert workflow session: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<string>(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in InsertSessionAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Error inserting session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates an existing workflow session
    /// </summary>
    /// <param name="session">Session entity with updated values</param>
    public async Task<Model_Dao_Result> UpdateSessionAsync(Model_Receiving_TableEntitys_WorkflowSession session)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(session);

            // Serialize load details to JSON if present
            string? loadDetailsJson = null;
            if (session.LoadDetailsJson != null)
            {
                loadDetailsJson = session.LoadDetailsJson;
            }

            var parameters = new Dictionary<string, object>
            {
                { "p_SessionId", session.SessionId.ToString() },
                { "p_CurrentStep", (int)session.CurrentStep },
                { "p_PONumber", (object?)session.PONumber ?? DBNull.Value },
                { "p_PartNumber", (object?)session.PartId ?? DBNull.Value },
                { "p_LoadCount", session.LoadCount },
                { "p_ReceivingLocationOverride", (object?)session.ReceivingLocationOverride ?? DBNull.Value },
                { "p_LoadDetailsJson", (object?)loadDetailsJson ?? DBNull.Value },
                { "p_ModifiedBy", $"User_{session.UserId}" }
            };

            _logger.LogInfo($"Updating workflow session: {session.SessionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_WorkflowSession_Update",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Workflow session updated successfully: {session.SessionId}");
            }
            else
            {
                _logger.LogError($"Failed to update workflow session: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpdateSessionAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a session by its ID
    /// </summary>
    /// <param name="sessionId">Session GUID</param>
    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>> SelectByIdAsync(string sessionId)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(sessionId);

            var parameters = new Dictionary<string, object>
            {
                { "p_SessionId", sessionId }
            };

            _logger.LogInfo($"Selecting workflow session by ID: {sessionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Receiving_WorkflowSession_SelectById",
                MapSession,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Workflow session found: {sessionId}");
            }
            else
            {
                _logger.LogWarning($"Workflow session not found: {sessionId}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByIdAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_WorkflowSession>(
                $"Error selecting session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all active sessions for a given user
    /// </summary>
    /// <param name="username">Username to filter by</param>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_WorkflowSession>>> SelectByUserAsync(string username)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(username);

            var parameters = new Dictionary<string, object>
            {
                { "p_Username", username }
            };

            _logger.LogInfo($"Selecting workflow sessions by user: {username}");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_WorkflowSession_SelectByUser",
                MapSession,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Found {result.Data?.Count ?? 0} sessions for user: {username}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByUserAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_WorkflowSession>>(
                $"Error selecting sessions: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps a database reader row to a WorkflowSession entity
    /// </summary>
    private static Model_Receiving_TableEntitys_WorkflowSession MapSession(IDataReader reader)
    {
        var session = new Model_Receiving_TableEntitys_WorkflowSession
        {
            SessionId = Guid.Parse(reader.GetString(reader.GetOrdinal("SessionId"))),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            CurrentStep = (Enum_Receiving_State_WorkflowStep)reader.GetInt32(reader.GetOrdinal("CurrentStep")),
            WorkflowMode = Enum.Parse<Enum_Receiving_Mode_WorkflowMode>(reader.GetString(reader.GetOrdinal("WorkflowMode"))),
            IsNonPO = reader.GetBoolean(reader.GetOrdinal("IsNonPO")),
            PONumber = reader.IsDBNull(reader.GetOrdinal("PONumber")) ? null : reader.GetString(reader.GetOrdinal("PONumber")),
            PartId = reader.IsDBNull(reader.GetOrdinal("PartId")) ? null : reader.GetString(reader.GetOrdinal("PartId")),
            LoadCount = reader.GetInt32(reader.GetOrdinal("LoadCount")),
            ReceivingLocationOverride = reader.IsDBNull(reader.GetOrdinal("ReceivingLocationOverride")) ? null : reader.GetString(reader.GetOrdinal("ReceivingLocationOverride")),
            LoadDetailsJson = reader.IsDBNull(reader.GetOrdinal("LoadDetailsJson")) ? null : reader.GetString(reader.GetOrdinal("LoadDetailsJson")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt"))
        };

        return session;
    }
}
