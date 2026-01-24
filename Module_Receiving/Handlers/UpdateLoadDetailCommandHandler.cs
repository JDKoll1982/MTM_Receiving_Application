using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for updating individual load detail data.
/// Clears auto-fill flags when user manually edits a field.
/// </summary>
public class UpdateLoadDetailCommandHandler : IRequestHandler<UpdateLoadDetailCommand, Result>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public UpdateLoadDetailCommandHandler(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateLoadDetailCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Updating load {LoadNumber} for session {SessionId}", request.LoadNumber, request.SessionId);

        // Get existing load to preserve auto-fill flags
        var existingLoadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!existingLoadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads for session {SessionId}: {Error}", request.SessionId, existingLoadsResult.ErrorMessage);
            return Result.Failure(existingLoadsResult.ErrorMessage);
        }

        var existingLoad = existingLoadsResult.Data?.FirstOrDefault(l => l.LoadNumber == request.LoadNumber);

        var load = new LoadDetail
        {
            SessionId = request.SessionId,
            LoadNumber = request.LoadNumber,
            Weight = request.Weight ?? 0,
            HeatLot = request.HeatLot ?? string.Empty,
            PackageType = request.PackageType ?? string.Empty,
            PackagesPerLoad = request.PackagesPerLoad ?? 0,
            // Clear auto-fill flags for manually edited fields
            IsWeightAutoFilled = existingLoad?.IsWeightAutoFilled == true && request.Weight == null ? true : false,
            IsHeatLotAutoFilled = existingLoad?.IsHeatLotAutoFilled == true && request.HeatLot == null ? true : false,
            IsPackageTypeAutoFilled = existingLoad?.IsPackageTypeAutoFilled == true && request.PackageType == null ? true : false,
            IsPackagesPerLoadAutoFilled = existingLoad?.IsPackagesPerLoadAutoFilled == true && request.PackagesPerLoad == null ? true : false
        };

        var result = await _loadDao.UpsertLoadDetailAsync(request.SessionId, load);

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to update load {LoadNumber} for session {SessionId}: {Error}",
                request.LoadNumber, request.SessionId, result.ErrorMessage);
            return Result.Failure(result.ErrorMessage);
        }

        _logger.Information("Successfully updated load {LoadNumber} for session {SessionId}", request.LoadNumber, request.SessionId);
        return Result.Success();
    }
}
