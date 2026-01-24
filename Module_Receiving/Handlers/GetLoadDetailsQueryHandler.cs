using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for retrieving all load details for a session.
/// Returns complete list of loads with all data and flags.
/// </summary>
public class GetLoadDetailsQueryHandler : IRequestHandler<GetLoadDetailsQuery, Result<List<LoadDetail>>>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public GetLoadDetailsQueryHandler(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<List<LoadDetail>>> Handle(GetLoadDetailsQuery request, CancellationToken cancellationToken)
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
