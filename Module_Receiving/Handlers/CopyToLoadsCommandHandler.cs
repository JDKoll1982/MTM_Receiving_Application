using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for bulk copy operations.
/// Copies specified fields from source load to target loads (empty cells only).
/// Sets auto-fill flags for copied data.
/// </summary>
public class CopyToLoadsCommandHandler : IRequestHandler<CopyToLoadsCommand, Result<CopyOperationResult>>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public CopyToLoadsCommandHandler(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<CopyOperationResult>> Handle(CopyToLoadsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Copying fields {Fields} from load {SourceLoad} to {TargetCount} target loads in session {SessionId}",
            request.Fields, request.SourceLoadNumber, request.TargetLoadNumbers.Count, request.SessionId);

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads for session {SessionId}: {Error}", request.SessionId, loadsResult.ErrorMessage);
            return Result<CopyOperationResult>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();
        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);

        if (sourceLoad == null)
        {
            _logger.Warning("Source load {SourceLoad} not found in session {SessionId}", request.SourceLoadNumber, request.SessionId);
            return Result<CopyOperationResult>.Failure($"Source load {request.SourceLoadNumber} not found");
        }

        // Perform copy operation via DAO
        var copyResult = await _loadDao.CopyToLoadsAsync(
            request.SessionId,
            request.SourceLoadNumber,
            request.TargetLoadNumbers,
            request.Fields
        );

        if (!copyResult.IsSuccess)
        {
            _logger.Error("Failed to copy to loads in session {SessionId}: {Error}", request.SessionId, copyResult.ErrorMessage);
            return Result<CopyOperationResult>.Failure(copyResult.ErrorMessage);
        }

        // Build result with statistics
        var result = new CopyOperationResult
        {
            SourceLoadNumber = request.SourceLoadNumber,
            TargetLoadNumbers = request.TargetLoadNumbers,
            CopiedFields = request.Fields,
            CellsCopied = CalculateCellsCopied(request.Fields, request.TargetLoadNumbers.Count),
            CellsSkipped = 0 // DAO only copies to empty cells, so skipped count handled there
        };

        _logger.Information("Copy operation completed: {CellsCopied} cells copied to {TargetCount} loads",
            result.CellsCopied, request.TargetLoadNumbers.Count);

        return Result<CopyOperationResult>.Success(result);
    }

    private int CalculateCellsCopied(CopyFields fields, int targetCount)
    {
        int fieldCount = 0;

        if ((fields & CopyFields.Weight) != 0) fieldCount++;
        if ((fields & CopyFields.HeatLot) != 0) fieldCount++;
        if ((fields & CopyFields.PackageType) != 0) fieldCount++;
        if ((fields & CopyFields.PackagesPerLoad) != 0) fieldCount++;

        return fieldCount * targetCount;
    }
}
