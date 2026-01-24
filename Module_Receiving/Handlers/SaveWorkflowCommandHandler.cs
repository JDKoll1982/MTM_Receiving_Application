using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;
using System.Globalization;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for saving completed workflow.
/// Validates all loads, generates CSV file, saves to database, and updates session.
/// </summary>
public class SaveWorkflowCommandHandler : IRequestHandler<SaveWorkflowCommand, Result<SaveResult>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public SaveWorkflowCommandHandler(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<SaveResult>> Handle(SaveWorkflowCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Saving workflow for session {SessionId} to {CSVPath}", request.SessionId, request.CSVPath);

        // Get session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, sessionResult.ErrorMessage);
            return Result<SaveResult>.Failure(sessionResult.ErrorMessage);
        }

        var session = sessionResult.Data;

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads for session {SessionId}: {Error}", request.SessionId, loadsResult.ErrorMessage);
            return Result<SaveResult>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();

        // Validate all loads before saving
        var validationResult = ValidateLoadsForSave(session, loads);
        if (!validationResult.IsSuccess)
        {
            _logger.Warning("Validation failed for session {SessionId}: {Error}", request.SessionId, validationResult.ErrorMessage);
            return Result<SaveResult>.Failure(validationResult.ErrorMessage);
        }

        // Generate CSV file
        string csvPath;
        try
        {
            csvPath = await GenerateCSVFile(request.CSVPath, session, loads);
            _logger.Information("CSV file generated: {CSVPath}", csvPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to generate CSV file for session {SessionId}", request.SessionId);
            return Result<SaveResult>.Failure($"Failed to generate CSV file: {ex.Message}");
        }

        // Save completed transaction to database
        var saveResult = await _loadDao.SaveCompletedTransactionAsync(
            request.SessionId,
            csvPath,
            request.User ?? Environment.UserName
        );

        if (!saveResult.IsSuccess)
        {
            _logger.Error("Failed to save completed transaction for session {SessionId}: {Error}",
                request.SessionId, saveResult.ErrorMessage);
            return Result<SaveResult>.Failure(saveResult.ErrorMessage);
        }

        // Mark session as saved
        session.IsSaved = true;
        var updateSessionResult = await _sessionDao.UpdateSessionAsync(session);
        if (!updateSessionResult.IsSuccess)
        {
            _logger.Warning("Failed to update session status to saved: {Error}", updateSessionResult.ErrorMessage);
            // Non-fatal - transaction is already saved
        }

        var result = new SaveResult
        {
            SessionId = request.SessionId,
            CSVPath = csvPath,
            LoadCount = loads.Count,
            SavedAt = DateTime.UtcNow
        };

        _logger.Information("Successfully saved workflow for session {SessionId}: {LoadCount} loads", request.SessionId, loads.Count);
        return Result<SaveResult>.Success(result);
    }

    private Result ValidateLoadsForSave(ReceivingWorkflowSession session, List<LoadDetail> loads)
    {
        // Validate session data
        if (string.IsNullOrWhiteSpace(session.PONumber))
            return Result.Failure("PO Number is required");

        if (session.PartId <= 0)
            return Result.Failure("Part must be selected");

        if (session.LoadCount <= 0)
            return Result.Failure("Load count must be greater than 0");

        // Validate load count matches
        if (loads.Count != session.LoadCount)
            return Result.Failure($"Expected {session.LoadCount} loads, but found {loads.Count}");

        // Validate each load
        foreach (var load in loads)
        {
            if (load.Weight <= 0)
                return Result.Failure($"Load {load.LoadNumber}: Weight is required");

            if (string.IsNullOrWhiteSpace(load.HeatLot))
                return Result.Failure($"Load {load.LoadNumber}: Heat Lot is required");

            if (string.IsNullOrWhiteSpace(load.PackageType))
                return Result.Failure($"Load {load.LoadNumber}: Package Type is required");

            if (load.PackagesPerLoad <= 0)
                return Result.Failure($"Load {load.LoadNumber}: Packages Per Load is required");
        }

        return Result.Success();
    }

    private async Task<string> GenerateCSVFile(string basePath, ReceivingWorkflowSession session, List<LoadDetail> loads)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(basePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Generate filename with timestamp if base path is directory
        var csvPath = basePath;
        if (Directory.Exists(basePath))
        {
            var filename = $"Receiving_{session.PONumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            csvPath = Path.Combine(basePath, filename);
        }

        // Write CSV using CsvHelper
        await using var writer = new StreamWriter(csvPath);
        await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        // Write header
        await csv.WriteRecordsAsync(loads.Select(load => new
        {
            PONumber = session.PONumber,
            PartNumber = $"PART-{session.PartId}", // TODO: Get actual part number from Part lookup
            LoadNumber = load.LoadNumber,
            Weight = load.Weight,
            HeatLot = load.HeatLot,
            PackageType = load.PackageType,
            PackagesPerLoad = load.PackagesPerLoad
        }));

        return csvPath;
    }
}
