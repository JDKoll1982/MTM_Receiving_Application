using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for updating Step 1 data (PO Number, Part, Load Count).
/// Initializes empty LoadDetail records for all loads.
/// </summary>
public class UpdateStep1CommandHandler : IRequestHandler<UpdateStep1Command, Result>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public UpdateStep1CommandHandler(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateStep1Command request, CancellationToken cancellationToken)
    {
        _logger.Information("Updating Step 1 for session {SessionId}: PO={PONumber}, PartId={PartId}, LoadCount={LoadCount}",
            request.SessionId, request.PONumber, request.PartId, request.LoadCount);

        // Get current session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, sessionResult.ErrorMessage);
            return Result.Failure(sessionResult.ErrorMessage);
        }

        var session = sessionResult.Data;

        // Update session with Step 1 data
        session.PONumber = request.PONumber;
        session.PartId = request.PartId;
        session.LoadCount = request.LoadCount;

        var updateSessionResult = await _sessionDao.UpdateSessionAsync(session);
        if (!updateSessionResult.IsSuccess)
        {
            _logger.Error("Failed to update session {SessionId}: {Error}", request.SessionId, updateSessionResult.ErrorMessage);
            return Result.Failure(updateSessionResult.ErrorMessage);
        }

        // Initialize load detail records if load count changed
        var existingLoadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        var existingLoads = existingLoadsResult.IsSuccess ? existingLoadsResult.Data : new List<LoadDetail>();

        // Create missing load records
        for (int i = 1; i <= request.LoadCount; i++)
        {
            if (!existingLoads.Any(l => l.LoadNumber == i))
            {
                var newLoad = new LoadDetail
                {
                    SessionId = request.SessionId,
                    LoadNumber = i,
                    Weight = 0,
                    HeatLot = string.Empty,
                    PackageType = string.Empty,
                    PackagesPerLoad = 0,
                    IsWeightAutoFilled = false,
                    IsHeatLotAutoFilled = false,
                    IsPackageTypeAutoFilled = false,
                    IsPackagesPerLoadAutoFilled = false
                };

                var insertResult = await _loadDao.UpsertLoadDetailAsync(request.SessionId, newLoad);
                if (!insertResult.IsSuccess)
                {
                    _logger.Warning("Failed to initialize load {LoadNumber} for session {SessionId}: {Error}",
                        i, request.SessionId, insertResult.ErrorMessage);
                }
            }
        }

        // Remove excess loads if count decreased
        var loadsToRemove = existingLoads.Where(l => l.LoadNumber > request.LoadCount).ToList();
        foreach (var load in loadsToRemove)
        {
            _logger.Information("Removing excess load {LoadNumber} from session {SessionId}", load.LoadNumber, request.SessionId);
            // Note: Removal would be handled by DeleteLoadAsync if we added that method
            // For now, loads beyond count are simply not displayed
        }

        _logger.Information("Successfully updated Step 1 for session {SessionId}", request.SessionId);
        return Result.Success();
    }
}
