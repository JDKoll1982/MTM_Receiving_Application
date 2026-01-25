using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for forcing an overwrite of occupied cells (power user feature).
/// Requires explicit user confirmation and logs all overwrites for audit trail.
/// </summary>
public class ForceOverwriteCommandHandler : IRequestHandler<ForceOverwriteCommand, Result<CopyOperationResult>>
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

    public async Task<Result<CopyOperationResult>> Handle(ForceOverwriteCommand request, CancellationToken cancellationToken)
    {
        if (!request.Confirmed)
        {
            _logger.Warning("Force overwrite attempted without confirmation for session {SessionId}", request.SessionId);
            return Result<CopyOperationResult>.Failure("Force overwrite requires explicit confirmation");
        }

        _logger.Warning("FORCE OVERWRITE: Overwriting {LoadCount} loads in session {SessionId} with fields {Fields}",
            request.TargetLoadNumbers.Count, request.SessionId, request.FieldsToOverwrite);

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads: {Error}", loadsResult.ErrorMessage);
            return Result<CopyOperationResult>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();
        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);

        if (sourceLoad == null)
        {
            _logger.Error("Source load {SourceLoad} not found", request.SourceLoadNumber);
            return Result<CopyOperationResult>.Failure($"Source load {request.SourceLoadNumber} not found");
        }

        // Perform overwrite operation
        int cellsOverwritten = 0;
        var loadsWithPreserved = new List<int>();

        foreach (var loadNumber in request.TargetLoadNumbers)
        {
            if (loadNumber == request.SourceLoadNumber)
                continue;

            var existingLoad = loads.FirstOrDefault(l => l.LoadNumber == loadNumber);
            if (existingLoad == null)
                continue;

            var targetLoad = new LoadDetail
            {
                LoadNumber = loadNumber,
                WeightOrQuantity = (request.FieldsToOverwrite == CopyFields.AllFields || request.FieldsToOverwrite == CopyFields.WeightOnly) 
                    ? sourceLoad.WeightOrQuantity 
                    : existingLoad.WeightOrQuantity,
                HeatLot = (request.FieldsToOverwrite == CopyFields.AllFields || request.FieldsToOverwrite == CopyFields.HeatLotOnly) 
                    ? sourceLoad.HeatLot 
                    : existingLoad.HeatLot,
                PackageType = (request.FieldsToOverwrite == CopyFields.AllFields || request.FieldsToOverwrite == CopyFields.PackageTypeOnly) 
                    ? sourceLoad.PackageType 
                    : existingLoad.PackageType,
                PackagesPerLoad = (request.FieldsToOverwrite == CopyFields.AllFields || request.FieldsToOverwrite == CopyFields.PackagesPerLoadOnly) 
                    ? sourceLoad.PackagesPerLoad 
                    : existingLoad.PackagesPerLoad,
                IsWeightAutoFilled = false,
                IsHeatLotAutoFilled = false,
                IsPackageTypeAutoFilled = false,
                IsPackagesPerLoadAutoFilled = false
            };

            var updateResult = await _loadDao.UpsertLoadDetailAsync(request.SessionId, targetLoad);
            if (!updateResult.IsSuccess)
            {
                _logger.Error("Failed to force overwrite load {LoadNumber}: {Error}", loadNumber, updateResult.ErrorMessage);
                return Result<CopyOperationResult>.Failure($"Failed to overwrite load {loadNumber}");
            }

            cellsOverwritten += CountFields(request.FieldsToOverwrite);
        }

        var result = new CopyOperationResult
        {
            SourceLoadNumber = request.SourceLoadNumber,
            TotalTargetLoads = request.TargetLoadNumbers.Count,
            CellsCopied = cellsOverwritten,
            CellsPreserved = 0,
            LoadsWithPreservedData = loadsWithPreserved,
            Success = true
        };

        _logger.Warning("FORCE OVERWRITE: Completed for session {SessionId}", request.SessionId);
        return Result<CopyOperationResult>.Success(result);
    }

    private int CountFields(CopyFields fields)
    {
        return fields switch
        {
            CopyFields.AllFields => 4,
            CopyFields.WeightOnly => 1,
            CopyFields.HeatLotOnly => 1,
            CopyFields.PackageTypeOnly => 1,
            CopyFields.PackagesPerLoadOnly => 1,
            _ => 0
        };
    }
}
