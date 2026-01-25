using System;
using System.Collections.Generic;
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
/// Handler for bulk copy operations from source load to multiple target loads.
/// Never overwrites existing data. Sets auto-fill flags for copied cells.
/// </summary>
public class Handler_ReceivingWizard_Copy_FieldsToEmptyCells : IRequestHandler<Command_ReceivingWizard_Copy_FieldsToEmptyCells, Result<CopyOperationResult>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_ReceivingWizard_Copy_FieldsToEmptyCells(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<CopyOperationResult>> Handle(Command_ReceivingWizard_Copy_FieldsToEmptyCells request, CancellationToken cancellationToken)
    {
        _logger.Information("Copying from load {SourceLoad} to {TargetLoadCount} loads with fields {Fields}", 
            request.SourceLoadNumber, request.TargetLoadNumbers.Count, request.FieldsToCopy);

        // Validate session exists
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Session {SessionId} not found", request.SessionId);
            return Result<CopyOperationResult>.Failure($"Session not found: {request.SessionId}");
        }

        var loads = (await _loadDao.GetLoadsBySessionAsync(request.SessionId)).Data ?? new List<LoadDetail>();

        // If no target loads specified, copy to all except source
        var targetLoads = request.TargetLoadNumbers.Count > 0 
            ? request.TargetLoadNumbers 
            : loads.FindAll(l => l.LoadNumber != request.SourceLoadNumber).ConvertAll(l => l.LoadNumber);

        // Find source load
        var sourceLoad = loads.Find(l => l.LoadNumber == request.SourceLoadNumber);
        if (sourceLoad == null)
        {
            _logger.Error("Source load {LoadNumber} not found", request.SourceLoadNumber);
            return Result<CopyOperationResult>.Failure($"Source load {request.SourceLoadNumber} not found");
        }

        // Perform copy operation via DAO
        var copyResult = await _loadDao.CopyToLoadsAsync(
            request.SessionId,
            request.SourceLoadNumber,
            targetLoads,
            request.FieldsToCopy);

        if (!copyResult.IsSuccess)
        {
            _logger.Error("Copy operation failed: {Error}", copyResult.ErrorMessage);
            return Result<CopyOperationResult>.Failure(copyResult.ErrorMessage);
        }

        // Calculate statistics
        var targetLoadObjects = loads.FindAll(l => targetLoads.Contains(l.LoadNumber));
        var cellsCopied = CalculateCellsCopied(request.FieldsToCopy, targetLoadObjects.Count);
        var loadsWithPreservedData = FindLoadsWithPreservedData(sourceLoad, targetLoadObjects, request.FieldsToCopy);

        var result = new CopyOperationResult
        {
            SourceLoadNumber = request.SourceLoadNumber,
            TotalTargetLoads = targetLoadObjects.Count,
            CellsCopied = cellsCopied,
            CellsPreserved = (targetLoadObjects.Count * CountFields(request.FieldsToCopy)) - cellsCopied,
            LoadsWithPreservedData = loadsWithPreservedData,
            Success = true
        };

        _logger.Information("Copy completed: {CellsCopied} cells copied, {CellsPreserved} preserved", 
            result.CellsCopied, result.CellsPreserved);
        
        return Result<CopyOperationResult>.Success(result);
    }

    private int CalculateCellsCopied(CopyFields fields, int targetLoadCount)
    {
        var fieldCount = CountFields(fields);
        return fieldCount * targetLoadCount;
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

    private List<int> FindLoadsWithPreservedData(LoadDetail sourceLoad, List<LoadDetail> targetLoads, CopyFields fields)
    {
        var loadsWithPreserved = new List<int>();

        foreach (var targetLoad in targetLoads)
        {
            bool hasPreservedData = false;

            if ((fields == CopyFields.AllFields || fields == CopyFields.WeightOnly) && targetLoad.IsWeightAutoFilled)
                hasPreservedData = true;
            if ((fields == CopyFields.AllFields || fields == CopyFields.HeatLotOnly) && targetLoad.IsHeatLotAutoFilled)
                hasPreservedData = true;
            if ((fields == CopyFields.AllFields || fields == CopyFields.PackageTypeOnly) && targetLoad.IsPackageTypeAutoFilled)
                hasPreservedData = true;
            if ((fields == CopyFields.AllFields || fields == CopyFields.PackagesPerLoadOnly) && targetLoad.IsPackagesPerLoadAutoFilled)
                hasPreservedData = true;

            if (hasPreservedData)
                loadsWithPreserved.Add(targetLoad.LoadNumber);
        }

        return loadsWithPreserved;
    }
}
