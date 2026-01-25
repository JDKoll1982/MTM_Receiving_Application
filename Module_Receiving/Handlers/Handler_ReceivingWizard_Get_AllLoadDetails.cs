using System;
using System.Collections.Generic;
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
/// Handler for retrieving all load details for a session.
/// Returns loads ordered by LoadNumber.
/// </summary>
public class Handler_Receiving_Wizard_Get_AllLoadDetails : IRequestHandler<Query_ReceivingWizard_Get_AllLoadDetails, Result<List<LoadDetail>>>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_Receiving_Wizard_Get_AllLoadDetails(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<List<LoadDetail>>> Handle(Query_ReceivingWizard_Get_AllLoadDetails request, CancellationToken cancellationToken)
    {
        _logger.Information("Retrieving all loads for session {SessionId}", request.SessionId);

        var result = await _loadDao.GetLoadsBySessionAsync(request.SessionId);

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads for session {SessionId}: {Error}", request.SessionId, result.ErrorMessage);
            return Result<List<LoadDetail>>.Failure(result.ErrorMessage);
        }

        var loads = result.Data ?? new List<LoadDetail>();
        _logger.Information("Retrieved {Count} loads for session {SessionId}", loads.Count, request.SessionId);

        return Result<List<LoadDetail>>.Success(loads);
    }
}
