using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for saving completed workflow.
/// Validates all loads, generates CSV file, and archives transaction record.
/// </summary>
public class Handler_ReceivingWizard_Data_SaveAndExportCSV : IRequestHandler<Command_ReceivingWizard_Data_SaveAndExportCSV, Result<SaveResult>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_ReceivingWizard_Data_SaveAndExportCSV(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<SaveResult>> Handle(Command_ReceivingWizard_Data_SaveAndExportCSV request, CancellationToken cancellationToken)
    {
        _logger.Information("Saving workflow: SessionId={SessionId}, CSVOutputPath={CSVPath}, SaveToDb={SaveToDb}",
            request.SessionId, request.CsvOutputPath, request.SaveToDatabase);

        // Get session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Session {SessionId} not found", request.SessionId);
            return Result<SaveResult>.Failure($"Session not found: {request.SessionId}");
        }

        var session = sessionResult.Data;

        // Get all loads
        var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        if (!loadsResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve loads: {Error}", loadsResult.ErrorMessage);
            return Result<SaveResult>.Failure(loadsResult.ErrorMessage);
        }

        var loads = loadsResult.Data ?? new List<LoadDetail>();

        // Validate all loads
        var validationResult = ValidateLoadsForSave(session, loads);
        if (!validationResult.IsSuccess)
        {
            _logger.Error("Validation failed: {Error}", validationResult.ErrorMessage);
            return Result<SaveResult>.Failure(validationResult.ErrorMessage);
        }

        // Generate CSV
        var csvPath = await GenerateCSVFileAsync(request.CsvOutputPath, session, loads);
        if (string.IsNullOrEmpty(csvPath))
        {
            _logger.Error("Failed to generate CSV");
            return Result<SaveResult>.Failure("Failed to generate CSV file");
        }

        // Save to database if requested
        if (request.SaveToDatabase)
        {
            var saveResult = await _loadDao.SaveCompletedTransactionAsync(
                request.SessionId,
                csvPath,
                Environment.UserName);

            if (!saveResult.IsSuccess)
            {
                _logger.Error("Failed to save transaction: {Error}", saveResult.ErrorMessage);
                return Result<SaveResult>.Failure($"Failed to save: {saveResult.ErrorMessage}");
            }
        }

        // Mark session as saved
        session.IsSaved = true;
        session.SavedAt = DateTime.UtcNow;
        session.SavedCsvPath = csvPath;

        var updateResult = await _sessionDao.UpdateSessionAsync(session);
        if (!updateResult.IsSuccess)
        {
            _logger.Error("Failed to update session: {Error}", updateResult.ErrorMessage);
            return Result<SaveResult>.Failure($"Failed to mark as saved: {updateResult.ErrorMessage}");
        }

        var result = new SaveResult(csvPath, 0, DateTime.UtcNow);
        _logger.Information("Workflow saved successfully: {CSVPath}", csvPath);

        return Result<SaveResult>.Success(result);
    }

    private Result ValidateLoadsForSave(ReceivingWorkflowSession session, List<LoadDetail> loads)
    {
        if (loads.Count != session.LoadCount)
        {
            return Result.Failure($"Load count mismatch: expected {session.LoadCount}, found {loads.Count}");
        }

        foreach (var load in loads)
        {
            if (load.WeightOrQuantity <= 0)
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

    private async Task<string> GenerateCSVFileAsync(string basePath, ReceivingWorkflowSession session, List<LoadDetail> loads)
    {
        try
        {
            var directory = Path.GetDirectoryName(basePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var filename = $"ReceivingWorkflow_{session.SessionId:N}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var csvPath = Path.Combine(basePath ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);

            await using var writer = new StreamWriter(csvPath);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write headers
            csv.WriteHeader<LoadDetail>();
            await csv.NextRecordAsync();

            // Write records
            foreach (var load in loads.OrderBy(l => l.LoadNumber))
            {
                csv.WriteRecord(load);
                await csv.NextRecordAsync();
            }

            _logger.Information("CSV file generated: {CSVPath}", csvPath);
            return csvPath;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error generating CSV: {Error}", ex.Message);
            return null;
        }
    }
}
