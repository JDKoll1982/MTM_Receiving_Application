using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for force overwrite operation (power user feature).
/// Copies data to all loads including occupied cells, with confirmation required.
/// Logs all overwrite operations for audit trail.
/// </summary>
public class ForceOverwriteCommandHandler : IRequestHandler<ForceOverwriteCommand, Result>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public ForceOverwriteCommandHandler(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(ForceOverwriteCommand request, CancellationToken cancellationToken)
    {
        if (!request.Confirmed)
        {
            _logger.Warning("Force overwrite attempted without confirmation for session {SessionId}", request.SessionId);
            return Result.Failure("Force overwrite requires explicit confirmation");
        }

        _logger.Warning("FORCE OVERWRITE: User {User} is overwriting {LoadCount} loads in session {SessionId} with fields {Fields}",
            request.User ?? "Unknown",
            request.TargetLoadNumbers.Count,
            request.SessionId,
            request.Fields);

        // Get all loads to capture current state for logging
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads for session {SessionId}: {Error}", request.SessionId, loadsResult.ErrorMessage);
            return Result.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();
        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);

        if (sourceLoad == null)
        {
            _logger.Error("Source load {SourceLoad} not found in session {SessionId}", request.SourceLoadNumber, request.SessionId);
            return Result.Failure($"Source load {request.SourceLoadNumber} not found");
        }

        // Log current values before overwrite (for audit trail)
        foreach (var loadNumber in request.TargetLoadNumbers)
        {
            var targetLoad = loads.FirstOrDefault(l => l.LoadNumber == loadNumber);
            if (targetLoad != null)
            {
                _logger.Warning("FORCE OVERWRITE AUDIT: Load {LoadNumber} before overwrite - Weight={Weight}, HeatLot={HeatLot}, PackageType={PackageType}, PackagesPerLoad={PackagesPerLoad}",
                    loadNumber, targetLoad.Weight, targetLoad.HeatLot, targetLoad.PackageType, targetLoad.PackagesPerLoad);
            }
        }

        // Perform overwrite operation (bypasses empty-cell-only restriction)
        // NOTE: This uses a special overwrite mode in the DAO that clears auto-fill flags and forces update
        foreach (var loadNumber in request.TargetLoadNumbers)
        {
            if (loadNumber == request.SourceLoadNumber)
                continue; // Skip source load

            var targetLoad = new LoadDetail
            {
                SessionId = request.SessionId,
                LoadNumber = loadNumber,
                Weight = (request.Fields & Models.Enums.CopyFields.Weight) != 0 ? sourceLoad.Weight : 0,
                HeatLot = (request.Fields & Models.Enums.CopyFields.HeatLot) != 0 ? sourceLoad.HeatLot : string.Empty,
                PackageType = (request.Fields & Models.Enums.CopyFields.PackageType) != 0 ? sourceLoad.PackageType : string.Empty,
                PackagesPerLoad = (request.Fields & Models.Enums.CopyFields.PackagesPerLoad) != 0 ? sourceLoad.PackagesPerLoad : 0,
                // Mark all copied fields as NOT auto-filled (force overwrite clears flags)
                IsWeightAutoFilled = false,
                IsHeatLotAutoFilled = false,
                IsPackageTypeAutoFilled = false,
                IsPackagesPerLoadAutoFilled = false
            };

            var updateResult = await _loadDao.UpsertLoadDetailAsync(request.SessionId, targetLoad);
            if (!updateResult.IsSuccess)
            {
                _logger.Error("Failed to force overwrite load {LoadNumber} in session {SessionId}: {Error}",
                    loadNumber, request.SessionId, updateResult.ErrorMessage);
                return Result.Failure($"Failed to overwrite load {loadNumber}: {updateResult.ErrorMessage}");
            }

            _logger.Warning("FORCE OVERWRITE COMPLETED: Load {LoadNumber} updated - Weight={Weight}, HeatLot={HeatLot}, PackageType={PackageType}, PackagesPerLoad={PackagesPerLoad}",
                loadNumber, targetLoad.Weight, targetLoad.HeatLot, targetLoad.PackageType, targetLoad.PackagesPerLoad);
        }

        _logger.Warning("FORCE OVERWRITE: Completed for session {SessionId} by user {User}",
            request.SessionId, request.User ?? "Unknown");

        return Result.Success();
    }
}
