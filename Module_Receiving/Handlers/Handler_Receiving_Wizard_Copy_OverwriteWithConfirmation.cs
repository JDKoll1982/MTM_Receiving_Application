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
public class Handler_Receiving_Wizard_Copy_OverwriteWithConfirmation : IRequestHandler<Command_Receiving_Wizard_Copy_OverwriteWithConfirmation, Result<Model_Receiving_Result_CopyOperation>>
{
    private readonly Dao_Receiving_Repository_LoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_Receiving_Wizard_Copy_OverwriteWithConfirmation(
        Dao_Receiving_Repository_LoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<Model_Receiving_Result_CopyOperation>> Handle(Command_Receiving_Wizard_Copy_OverwriteWithConfirmation request, CancellationToken cancellationToken)
    {
        if (!request.Confirmed)
        {
            _logger.Warning("Force overwrite attempted without confirmation for session {SessionId}", request.SessionId);
            return Result<Model_Receiving_Result_CopyOperation>.Failure("Force overwrite requires explicit confirmation");
        }

        _logger.Warning("FORCE OVERWRITE: Overwriting {LoadCount} loads in session {SessionId} with fields {Fields}",
            request.TargetLoadNumbers.Count, request.SessionId, request.FieldsToOverwrite);

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads: {Error}", loadsResult.ErrorMessage);
            return Result<Model_Receiving_Result_CopyOperation>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<Model_Receiving_Entity_LoadDetail>();
        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);

        if (sourceLoad == null)
        {
            _logger.Error("Source load {SourceLoad} not found", request.SourceLoadNumber);
            return Result<Model_Receiving_Result_CopyOperation>.Failure($"Source load {request.SourceLoadNumber} not found");
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

            var targetLoad = new Model_Receiving_Entity_LoadDetail
            {
                LoadNumber = loadNumber,
                WeightOrQuantity = (request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.AllFields || request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.WeightOnly) 
                    ? sourceLoad.WeightOrQuantity 
                    : existingLoad.WeightOrQuantity,
                HeatLot = (request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.AllFields || request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.HeatLotOnly) 
                    ? sourceLoad.HeatLot 
                    : existingLoad.HeatLot,
                PackageType = (request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.AllFields || request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.PackageTypeOnly) 
                    ? sourceLoad.PackageType 
                    : existingLoad.PackageType,
                PackagesPerLoad = (request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.AllFields || request.FieldsToOverwrite == Enum_Receiving_Type_CopyFields.PackagesPerLoadOnly) 
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
                return Result<Model_Receiving_Result_CopyOperation>.Failure($"Failed to overwrite load {loadNumber}");
            }

            cellsOverwritten += CountFields(request.FieldsToOverwrite);
        }

        var result = new Model_Receiving_Result_CopyOperation
        {
            SourceLoadNumber = request.SourceLoadNumber,
            TotalTargetLoads = request.TargetLoadNumbers.Count,
            CellsCopied = cellsOverwritten,
            CellsPreserved = 0,
            LoadsWithPreservedData = loadsWithPreserved,
            Success = true
        };

        _logger.Warning("FORCE OVERWRITE: Completed for session {SessionId}", request.SessionId);
        return Result<Model_Receiving_Result_CopyOperation>.Success(result);
    }

    private int CountFields(Enum_Receiving_Type_CopyFields fields)
    {
        return fields switch
        {
            Enum_Receiving_Type_CopyFields.AllFields => 4,
            Enum_Receiving_Type_CopyFields.WeightOnly => 1,
            Enum_Receiving_Type_CopyFields.HeatLotOnly => 1,
            Enum_Receiving_Type_CopyFields.PackageTypeOnly => 1,
            Enum_Receiving_Type_CopyFields.PackagesPerLoadOnly => 1,
            _ => 0
        };
    }
}
