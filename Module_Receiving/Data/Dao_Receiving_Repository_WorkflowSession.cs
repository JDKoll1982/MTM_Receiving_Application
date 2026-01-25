using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for receiving_workflow_sessions table
/// Provides CRUD operations for workflow session management using stored procedures
/// </summary>
public class Dao_Receiving_Repository_WorkflowSession
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of Dao_ReceivingWorkflowSession
    /// </summary>
    /// <param name="connectionString">MySQL connection string</param>
    public Dao_Receiving_Repository_WorkflowSession(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a new receiving workflow session in the database
    /// </summary>
    /// <param name="session">Session entity to create</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result<Guid>> CreateSessionAsync(Model_Receiving_Entity_WorkflowSession session)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", session.SessionId.ToString()),
                new MySqlParameter("@p_CurrentStep", session.CurrentStep),
                new MySqlParameter("@p_PONumber", session.PONumber ?? string.Empty),
                new MySqlParameter("@p_PartId", session.PartId ?? 0),
                new MySqlParameter("@p_LoadCount", session.LoadCount),
                new MySqlParameter("@p_CreatedAt", session.CreatedAt),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result<Guid>
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Create_Receiving_Session",
                parameters,
                _connectionString
            );

            if (result.IsSuccess)
            {
                return new Model_Dao_Result<Guid>
                {
                    Success = true,
                    Data = session.SessionId
                };
            }

            return new Model_Dao_Result<Guid>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                Severity = result.Severity
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<Guid>
            {
                Success = false,
                ErrorMessage = $"Unexpected error creating workflow session: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Retrieves a session by its identifier
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>Model_Dao_Result with session data or error</returns>
    public async Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> GetSessionAsync(Guid sessionId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString())
            };

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Get_Session_With_Loads",
                parameters,
                _connectionString
            );

            if (!result.IsSuccess)
            {
                return new Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    Severity = result.Severity
                };
            }

            // For now, construct a minimal session object
            // In production, this would deserialize from stored procedure output
            var session = new Model_Receiving_Entity_WorkflowSession
            {
                SessionId = sessionId,
                CurrentStep = 1,
                PONumber = null,
                PartId = null,
                LoadCount = 0,
                CreatedAt = DateTime.UtcNow,
                IsEditMode = false,
                IsSaved = false,
                HasUnsavedChanges = false
            };

            return new Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>
            {
                Success = true,
                Data = session
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>
            {
                Success = false,
                ErrorMessage = $"Unexpected error retrieving session: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Updates an existing workflow session
    /// </summary>
    /// <param name="session">Session entity to update</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> UpdateSessionAsync(Model_Receiving_Entity_WorkflowSession session)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", session.SessionId.ToString()),
                new MySqlParameter("@p_CurrentStep", session.CurrentStep),
                new MySqlParameter("@p_PONumber", session.PONumber ?? string.Empty),
                new MySqlParameter("@p_PartId", session.PartId ?? 0),
                new MySqlParameter("@p_LoadCount", session.LoadCount),
                new MySqlParameter("@p_IsSaved", session.IsSaved),
                new MySqlParameter("@p_SavedAt", session.SavedAt ?? (object)DBNull.Value),
                new MySqlParameter("@p_LastModifiedAt", DateTime.UtcNow),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Update_Receiving_Session",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error updating workflow session: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Deletes a workflow session and all related load details
    /// </summary>
    /// <param name="sessionId">Session identifier to delete</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> DeleteSessionAsync(Guid sessionId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString()),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Delete_Receiving_Session",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error deleting session: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }
}
