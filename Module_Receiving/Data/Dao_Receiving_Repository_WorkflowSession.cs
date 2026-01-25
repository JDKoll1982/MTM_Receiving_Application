using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data access object for workflow session operations
/// Manages session state persistence for the 3-step wizard
/// </summary>
public class Dao_Receiving_Repository_WorkflowSession
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_WorkflowSession(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Insert or update workflow session state
    /// </summary>
    /// <param name="session"></param>
    public async Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> UpsertSessionAsync(
        Model_Receiving_Entity_WorkflowSession session)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("sp_Receiving_WorkflowSession_Upsert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@SessionId", session.SessionId);
            command.Parameters.AddWithValue("@UserId", session.UserId);
            command.Parameters.AddWithValue("@CurrentStep", (int)session.CurrentStep);
            command.Parameters.AddWithValue("@WorkflowMode", session.WorkflowMode.ToString());
            command.Parameters.AddWithValue("@IsNonPO", session.IsNonPO);
            command.Parameters.AddWithValue("@PONumber", (object?)session.PONumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@PartId", (object?)session.PartId ?? DBNull.Value);
            command.Parameters.AddWithValue("@LoadCount", session.LoadCount);
            command.Parameters.AddWithValue("@ReceivingLocationOverride", (object?)session.ReceivingLocationOverride ?? DBNull.Value);
            command.Parameters.AddWithValue("@LoadDetailsJson", (object?)session.LoadDetailsJson ?? DBNull.Value);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var result = reader["Result"].ToString();
                return Model_Dao_Result_Factory.Success(session, 1);
            }

            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
                "Failed to save session - no result returned");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
                $"Error saving workflow session: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve workflow session by session ID
    /// </summary>
    /// <param name="sessionId"></param>
    public async Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> GetSessionByIdAsync(Guid sessionId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("sp_Receiving_WorkflowSession_SelectBySessionId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@SessionId", sessionId);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var session = MapReaderToSession(reader);
                return Model_Dao_Result_Factory.Success(session, 1);
            }

            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
                "Session not found or expired");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
                $"Error retrieving session: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve active workflow session for a user
    /// </summary>
    /// <param name="userId"></param>
    public async Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> GetActiveSessionByUserIdAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("sp_Receiving_WorkflowSession_SelectByUserId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var session = MapReaderToSession(reader);
                return Model_Dao_Result_Factory.Success(session, 1);
            }

            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
                "No active session found for user");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
                $"Error retrieving active session: {ex.Message}");
        }
    }

    private static Model_Receiving_Entity_WorkflowSession MapReaderToSession(SqlDataReader reader)
    {
        return new Model_Receiving_Entity_WorkflowSession
        {
            SessionId = reader.GetGuid(reader.GetOrdinal("session_id")),
            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
            CurrentStep = (Enum_Receiving_State_WorkflowStep)reader.GetInt32(reader.GetOrdinal("current_step")),
            WorkflowMode = Enum.Parse<Enum_Receiving_Mode_WorkflowMode>(reader.GetString(reader.GetOrdinal("workflow_mode"))),
            IsNonPO = reader.GetBoolean(reader.GetOrdinal("is_non_po")),
            PONumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? null : reader.GetString(reader.GetOrdinal("po_number")),
            PartId = reader.IsDBNull(reader.GetOrdinal("part_id")) ? null : reader.GetString(reader.GetOrdinal("part_id")),
            LoadCount = reader.GetInt32(reader.GetOrdinal("load_count")),
            ReceivingLocationOverride = reader.IsDBNull(reader.GetOrdinal("receiving_location_override")) ? null : reader.GetString(reader.GetOrdinal("receiving_location_override")),
            LoadDetailsJson = reader.IsDBNull(reader.GetOrdinal("load_details_json")) ? null : reader.GetString(reader.GetOrdinal("load_details_json")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at"))
        };
    }
}
