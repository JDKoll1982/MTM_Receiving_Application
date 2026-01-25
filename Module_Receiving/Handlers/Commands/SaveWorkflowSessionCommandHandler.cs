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
/// Handler for SaveWorkflowSessionCommand
/// Persists wizard session state between steps
/// </summary>
public class SaveWorkflowSessionCommandHandler
    : IRequestHandler<SaveWorkflowSessionCommand, Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>>
{
    private readonly Dao_Receiving_Repository_WorkflowSession _sessionDao;

    public SaveWorkflowSessionCommandHandler(Dao_Receiving_Repository_WorkflowSession sessionDao)
    {
        _sessionDao = sessionDao;
    }

    public async Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> Handle(
        SaveWorkflowSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = new Model_Receiving_Entity_WorkflowSession
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

        return await _sessionDao.UpsertSessionAsync(session);
    }
}
