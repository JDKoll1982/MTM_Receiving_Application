using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for previewing copy operation effects.
/// Shows which cells will be copied and which will be skipped.
/// </summary>
public class GetCopyPreviewQueryHandler : IRequestHandler<GetCopyPreviewQuery, Result<CopyPreview>>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public GetCopyPreviewQueryHandler(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<CopyPreview>> Handle(GetCopyPreviewQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("Generating copy preview for session {SessionId}, source load {SourceLoad}, fields {Fields}",
            request.SessionId, request.SourceLoadNumber, request.Fields);

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads for session {SessionId}: {Error}", request.SessionId, loadsResult.ErrorMessage);
            return Result<CopyPreview>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();
        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);

        if (sourceLoad == null)
        {
            _logger.Warning("Source load {SourceLoad} not found in session {SessionId}", request.SourceLoadNumber, request.SessionId);
            return Result<CopyPreview>.Failure($"Source load {request.SourceLoadNumber} not found");
        }

        // Calculate which cells will be copied vs skipped
        var targetLoads = loads.Where(l => request.TargetLoadNumbers.Contains(l.LoadNumber)).ToList();
        int cellsToBeCopied = 0;
        int cellsToBeSkipped = 0;

        foreach (var targetLoad in targetLoads)
        {
            if (targetLoad.LoadNumber == request.SourceLoadNumber)
                continue; // Skip source load itself

            if ((request.Fields & CopyFields.Weight) != 0)
            {
                if (targetLoad.Weight == 0) cellsToBeCopied++;
                else cellsToBeSkipped++;
            }

            if ((request.Fields & CopyFields.HeatLot) != 0)
            {
                if (string.IsNullOrWhiteSpace(targetLoad.HeatLot)) cellsToBeCopied++;
                else cellsToBeSkipped++;
            }

            if ((request.Fields & CopyFields.PackageType) != 0)
            {
                if (string.IsNullOrWhiteSpace(targetLoad.PackageType)) cellsToBeCopied++;
                else cellsToBeSkipped++;
            }

            if ((request.Fields & CopyFields.PackagesPerLoad) != 0)
            {
                if (targetLoad.PackagesPerLoad == 0) cellsToBeCopied++;
                else cellsToBeSkipped++;
            }
        }

        var preview = new CopyPreview
        {
            SourceLoadNumber = request.SourceLoadNumber,
            TargetLoadNumbers = request.TargetLoadNumbers,
            FieldsToCopy = request.Fields,
            CellsThatWillBeCopied = cellsToBeCopied,
            CellsThatWillBeSkipped = cellsToBeSkipped
        };

        _logger.Information("Copy preview generated: {CellsCopied} cells will be copied, {CellsSkipped} will be skipped",
            cellsToBeCopied, cellsToBeSkipped);

        return Result<CopyPreview>.Success(preview);
    }
}
