using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for updating Step 2 (Load Details Entry).
/// Updates individual load data and clears auto-fill flags for manually edited fields.
/// </summary>
public class Handler_Receiving_Wizard_Data_UpdateLoadEntry : IRequestHandler<Command_Receiving_Wizard_Data_UpdateLoadEntry, Result>
{
    private readonly Dao_Receiving_Repository_LoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_Receiving_Wizard_Data_UpdateLoadEntry(
        Dao_Receiving_Repository_LoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(Command_Receiving_Wizard_Data_UpdateLoadEntry request, CancellationToken cancellationToken)
    {
        _logger.Information("Updating load detail: Session={SessionId}, Load={LoadNumber}", 
            request.SessionId, request.LoadNumber);

        // Get existing load
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads: {Error}", loadsResult.ErrorMessage);
            return Result.Failure(loadsResult.ErrorMessage);
        }

        var existingLoad = loadsResult.Data?.FirstOrDefault(l => l.LoadNumber == request.LoadNumber);
        if (existingLoad == null)
        {
            _logger.Error("Load {LoadNumber} not found", request.LoadNumber);
            return Result.Failure($"Load {request.LoadNumber} not found");
        }

        // Create updated load (only update provided fields)
        var updatedLoad = new Model_Receiving_Entity_LoadDetail
        {
            LoadNumber = request.LoadNumber,
            WeightOrQuantity = request.WeightOrQuantity ?? existingLoad.WeightOrQuantity,
            HeatLot = request.HeatLot ?? existingLoad.HeatLot,
            PackageType = request.PackageType ?? existingLoad.PackageType,
            PackagesPerLoad = request.PackagesPerLoad ?? existingLoad.PackagesPerLoad,
            // Clear auto-fill flags for fields that were manually edited
            IsWeightAutoFilled = request.WeightOrQuantity == null ? existingLoad.IsWeightAutoFilled : false,
            IsHeatLotAutoFilled = request.HeatLot == null ? existingLoad.IsHeatLotAutoFilled : false,
            IsPackageTypeAutoFilled = request.PackageType == null ? existingLoad.IsPackageTypeAutoFilled : false,
            IsPackagesPerLoadAutoFilled = request.PackagesPerLoad == null ? existingLoad.IsPackagesPerLoadAutoFilled : false
        };

        var updateResult = await _loadDao.UpsertLoadDetailAsync(request.SessionId, updatedLoad);
        if (!updateResult.IsSuccess)
        {
            _logger.Error("Failed to update load: {Error}", updateResult.ErrorMessage);
            return Result.Failure(updateResult.ErrorMessage);
        }

        _logger.Information("Successfully updated load {LoadNumber}", request.LoadNumber);
        return Result.Success();
    }
}
