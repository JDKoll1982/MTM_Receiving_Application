using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for previewing copy operations.
/// Shows what cells will be copied vs preserved before execution.
/// </summary>
public class Handler_Receiving_Wizard_Preview_CopyOperation : IRequestHandler<Query_ReceivingWizard_Preview_CopyOperation, Result<CopyPreview>>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_Receiving_Wizard_Preview_CopyOperation(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<CopyPreview>> Handle(Query_ReceivingWizard_Preview_CopyOperation request, CancellationToken cancellationToken)
    {
        _logger.Information("Previewing copy operation: Session={SessionId}, Source={SourceLoad}, Targets={TargetCount}, Fields={Fields}",
            request.SessionId, request.SourceLoadNumber, request.TargetLoadNumbers.Count, request.FieldsToCopy);

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads: {Error}", loadsResult.ErrorMessage);
            return Result<CopyPreview>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();
        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);

        if (sourceLoad == null)
        {
            _logger.Error("Source load {SourceLoad} not found", request.SourceLoadNumber);
            return Result<CopyPreview>.Failure($"Source load {request.SourceLoadNumber} not found");
        }

        // Calculate preview
        var targetLoads = request.TargetLoadNumbers.Count > 0
            ? loads.Where(l => request.TargetLoadNumbers.Contains(l.LoadNumber)).ToList()
            : loads.Where(l => l.LoadNumber != request.SourceLoadNumber).ToList();

        int cellsToBeCopied = 0;
        int cellsToBePreserved = 0;
        var conflictLoads = new Dictionary<int, List<string>>();

        foreach (var targetLoad in targetLoads)
        {
            var conflictingFields = new List<string>();

            // Check which fields will be copied vs preserved
            if (request.FieldsToCopy == CopyFields.AllFields || request.FieldsToCopy == CopyFields.WeightOnly)
            {
                if (targetLoad.WeightOrQuantity != null && targetLoad.WeightOrQuantity > 0)
                    conflictingFields.Add("WeightOrQuantity");
                else
                    cellsToBeCopied++;
            }

            if (request.FieldsToCopy == CopyFields.AllFields || request.FieldsToCopy == CopyFields.HeatLotOnly)
            {
                if (!string.IsNullOrWhiteSpace(targetLoad.HeatLot))
                    conflictingFields.Add("HeatLot");
                else
                    cellsToBeCopied++;
            }

            if (request.FieldsToCopy == CopyFields.AllFields || request.FieldsToCopy == CopyFields.PackageTypeOnly)
            {
                if (!string.IsNullOrWhiteSpace(targetLoad.PackageType))
                    conflictingFields.Add("PackageType");
                else
                    cellsToBeCopied++;
            }

            if (request.FieldsToCopy == CopyFields.AllFields || request.FieldsToCopy == CopyFields.PackagesPerLoadOnly)
            {
                if (targetLoad.PackagesPerLoad != null && targetLoad.PackagesPerLoad > 0)
                    conflictingFields.Add("PackagesPerLoad");
                else
                    cellsToBeCopied++;
            }

            cellsToBePreserved += conflictingFields.Count;

            if (conflictingFields.Count > 0)
                conflictLoads[targetLoad.LoadNumber] = conflictingFields;
        }

        var preview = new CopyPreview
        {
            CellsToBeCopied = cellsToBeCopied,
            CellsToBePreserved = cellsToBePreserved,
            LoadsWithConflicts = conflictLoads
        };

        _logger.Information("Copy preview: {CellsToBeCopied} to copy, {CellsPreserved} to preserve, {ConflictLoads} loads with conflicts",
            cellsToBeCopied, cellsToBePreserved, conflictLoads.Count);

        return Result<CopyPreview>.Success(preview);
    }
}
