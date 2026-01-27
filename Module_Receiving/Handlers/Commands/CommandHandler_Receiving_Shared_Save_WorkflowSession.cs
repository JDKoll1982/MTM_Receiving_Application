using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

/// <summary>
/// <para>
/// This handler processes a save command for workflow sessions, creating a new session entity
/// with the provided data (session ID, user ID, current step, workflow mode, and other details).
/// It checks if a session already exists by attempting to select it; if it does, it updates the existing
/// session; otherwise, it inserts a new one. The handler returns a result indicating success or failure.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_WorkflowSession: For database operations on workflow sessions (select, update, insert)
/// - CommandRequest_Receiving_Shared_Save_WorkflowSession: The command this handler processes
/// - Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>: The result type returned by the handler
/// - Model_Dao_Result_Factory: For creating failure results
/// </para>
/// </summary>
public class CommandHandler_Receiving_Shared_Save_WorkflowSession
    : IRequestHandler<CommandRequest_Receiving_Shared_Save_WorkflowSession, Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>>
{
    private readonly Dao_Receiving_Repository_WorkflowSession _sessionDao;

    public CommandHandler_Receiving_Shared_Save_WorkflowSession(Dao_Receiving_Repository_WorkflowSession sessionDao)
    {
        _sessionDao = sessionDao;
    }

    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>> Handle(
        CommandRequest_Receiving_Shared_Save_WorkflowSession request,
        CancellationToken cancellationToken)
    {
        var session = new Model_Receiving_TableEntitys_WorkflowSession
        {
            SessionId = request.SessionId == Guid.Empty ? Guid.NewGuid() : request.SessionId,
            UserId = request.UserId,
            CurrentStep = (Enum_Receiving_State_WorkflowStep)request.CurrentStep,
            WorkflowMode = Enum_Receiving_Mode_WorkflowMode.Wizard,
            IsNonPO = request.IsNonPO,
            PONumber = request.PONumber,
            PartId = request.PartId,
            LoadCount = request.LoadCount,
            ReceivingLocationOverride = request.ReceivingLocationOverride,
            LoadDetailsJson = request.LoadDetailsJson
        };

        // Check if session exists by trying to select it
        if (request.SessionId != Guid.Empty)
        {
            var existing = await _sessionDao.SelectByIdAsync(request.SessionId.ToString());
            if (existing.Success)
            {
                // Update existing session
                var updateResult = await _sessionDao.UpdateSessionAsync(session);
                if (updateResult.Success)
                {
                    return new Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>
                    {
                        Success = true,
                        Data = session
                    };
                }
                return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_WorkflowSession>(updateResult.ErrorMessage);
            }
        }

        // Insert new session
        var insertResult = await _sessionDao.InsertSessionAsync(session);
        if (insertResult.Success)
        {
            return new Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>
            {
                Success = true,
                Data = session
            };
        }

        return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_WorkflowSession>(insertResult.ErrorMessage);
    }
}
