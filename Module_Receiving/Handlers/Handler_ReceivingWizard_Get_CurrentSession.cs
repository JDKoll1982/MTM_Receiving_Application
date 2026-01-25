using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for retrieving workflow session state.
/// Returns current session with all metadata.
/// </summary>
public class Handler_ReceivingWizard_Get_CurrentSession : IRequestHandler<Query_ReceivingWizard_Get_CurrentSession, Result<ReceivingWorkflowSession>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly ILogger _logger;

    public Handler_ReceivingWizard_Get_CurrentSession(
        Dao_ReceivingWorkflowSession sessionDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _logger = logger;
    }

    public async Task<Result<ReceivingWorkflowSession>> Handle(Query_ReceivingWizard_Get_CurrentSession request, CancellationToken cancellationToken)
    {
        _logger.Information("Retrieving session {SessionId}", request.SessionId);

        var result = await _sessionDao.GetSessionAsync(request.SessionId);

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, result.ErrorMessage);
            return Result<ReceivingWorkflowSession>.Failure(result.ErrorMessage);
        }

        if (result.Data == null)
        {
            _logger.Warning("Session {SessionId} not found", request.SessionId);
            return Result<ReceivingWorkflowSession>.Failure($"Session not found: {request.SessionId}");
        }

        _logger.Information("Successfully retrieved session {SessionId}", request.SessionId);
        return Result<ReceivingWorkflowSession>.Success(result.Data);
    }
}
