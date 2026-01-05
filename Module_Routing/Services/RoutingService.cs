using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Core service for routing label operations: creation, editing, CSV export, validation
/// </summary>
public class RoutingService : IRoutingService
{
    private readonly Dao_RoutingLabel _daoLabel;
    private readonly Dao_RoutingLabelHistory _daoHistory;
    private readonly IRoutingInforVisualService _inforVisualService;
    private readonly IRoutingUsageTrackingService _usageTrackingService;
    private readonly ILoggingService _logger;
    private readonly IConfiguration _configuration;

    public RoutingService(
        Dao_RoutingLabel daoLabel,
        Dao_RoutingLabelHistory daoHistory,
        IRoutingInforVisualService inforVisualService,
        IRoutingUsageTrackingService usageTrackingService,
        ILoggingService logger,
        IConfiguration configuration)
    {
        _daoLabel = daoLabel ?? throw new ArgumentNullException(nameof(daoLabel));
        _daoHistory = daoHistory ?? throw new ArgumentNullException(nameof(daoHistory));
        _inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _usageTrackingService = usageTrackingService ?? throw new ArgumentNullException(nameof(usageTrackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Model_Dao_Result<int>> CreateLabelAsync(Model_RoutingLabel label)
    {
        try
        {
            await _logger.LogInformationAsync($"Creating routing label for PO {label.PONumber}, Recipient {label.RecipientId}");

            // Validate label
            var validationResult = ValidateLabel(label);
            if (!validationResult.IsSuccess)
            {
                return Model_Dao_Result<int>.Failure(validationResult.ErrorMessage);
            }

            // Check for duplicates
            var duplicateResult = await CheckDuplicateLabelAsync(
                label.PONumber,
                label.LineNumber,
                label.RecipientId,
                label.CreatedDate
            );
            
            if (duplicateResult.IsSuccess && duplicateResult.Data.Exists)
            {
                return Model_Dao_Result<int>.Failure(
                    $"Duplicate label exists (ID: {duplicateResult.Data.ExistingLabelId})"
                );
            }

            // Insert label
            var insertResult = await _daoLabel.InsertAsync(label);
            if (!insertResult.IsSuccess)
            {
                return insertResult;
            }

            int labelId = insertResult.Data;

            // Export to CSV (async with retry)
            label.Id = labelId;
            var csvResult = await ExportLabelToCsvAsync(label);
            if (!csvResult.IsSuccess)
            {
                await _logger.LogWarningAsync($"CSV export failed for label {labelId}: {csvResult.ErrorMessage}");
                // Continue - label is saved, CSV can be regenerated later
            }

            // Increment usage tracking
            var usageResult = await _usageTrackingService.IncrementUsageCountAsync(
                label.EmployeeNumber,
                label.RecipientId
            );
            if (!usageResult.IsSuccess)
            {
                await _logger.LogWarningAsync($"Usage tracking failed: {usageResult.ErrorMessage}");
                // Continue - not critical
            }

            return Model_Dao_Result<int>.Success(labelId, "Label created successfully", 1);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error creating label: {ex.Message}", ex);
            return Model_Dao_Result<int>.Failure($"Error creating label: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label, int editedByEmployeeNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Updating label {label.Id} by employee {editedByEmployeeNumber}");

            // Get original label for history tracking
            var originalResult = await _daoLabel.GetByIdAsync(label.Id);
            if (!originalResult.IsSuccess)
            {
                return Model_Dao_Result.Failure($"Original label not found: {originalResult.ErrorMessage}");
            }

            var original = originalResult.Data;

            // Validate updated label
            var validationResult = ValidateLabel(label);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // Update label
            var updateResult = await _daoLabel.UpdateAsync(label);
            if (!updateResult.IsSuccess)
            {
                return updateResult;
            }

            // Log changes to history
            var changes = GetLabelChanges(original, label);
            foreach (var (fieldName, oldValue, newValue) in changes)
            {
                var history = new Model_RoutingLabelHistory
                {
                    LabelId = label.Id,
                    FieldName = fieldName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    EditedByEmployeeNumber = editedByEmployeeNumber,
                    EditedDate = DateTime.Now
                };

                await _daoHistory.InsertHistoryAsync(history);
            }

            return Model_Dao_Result.Success("Label updated successfully", 1);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error updating label: {ex.Message}", ex);
            return Model_Dao_Result.Failure($"Error updating label: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId)
    {
        try
        {
            return await _daoLabel.GetByIdAsync(labelId);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting label {labelId}: {ex.Message}", ex);
            return Model_Dao_Result<Model_RoutingLabel>.Failure($"Error getting label: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0)
    {
        try
        {
            return await _daoLabel.GetAllAsync(limit, offset);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting labels: {ex.Message}", ex);
            return Model_Dao_Result<List<Model_RoutingLabel>>.Failure($"Error getting labels: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateLabelAsync(
        string poNumber,
        string lineNumber,
        int recipientId,
        DateTime createdDate)
    {
        try
        {
            var result = await _daoLabel.CheckDuplicateAsync(poNumber, lineNumber, recipientId, createdDate);
            
            if (result.IsSuccess)
            {
                bool exists = result.Data > 0;
                int? existingId = exists ? (int?)result.Data : null;
                return Model_Dao_Result<(bool, int?)>.Success(
                    (exists, existingId),
                    exists ? "Duplicate found" : "No duplicate",
                    1
                );
            }
            
            return Model_Dao_Result<(bool, int?)>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error checking duplicate: {ex.Message}", ex);
            return Model_Dao_Result<(bool, int?)>.Failure($"Error checking duplicate: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> ExportLabelToCsvAsync(Model_RoutingLabel label)
    {
        try
        {
            // Get CSV export paths from configuration
            var networkPath = _configuration["RoutingModule:CsvExportPath:Network"];
            var localPath = _configuration["RoutingModule:CsvExportPath:Local"];
            var retryCount = _configuration.GetValue<int>("RoutingModule:CsvRetry:MaxAttempts", 3);
            var retryDelay = _configuration.GetValue<int>("RoutingModule:CsvRetry:DelayMs", 500);

            // Format CSV line
            var csvLine = FormatCsvLine(label);

            // Try network path first with retry
            bool networkSuccess = await TryWriteCsvAsync(networkPath, csvLine, retryCount, retryDelay);

            // Try local path as fallback
            bool localSuccess = false;
            if (!networkSuccess)
            {
                await _logger.LogWarningAsync($"Network CSV export failed, using local path: {localPath}");
                localSuccess = await TryWriteCsvAsync(localPath, csvLine, retryCount, retryDelay);
            }

            if (networkSuccess || localSuccess)
            {
                // Mark as exported in database
                await _daoLabel.MarkExportedAsync(label.Id);
                
                return Model_Dao_Result.Success(
                    networkSuccess ? "Exported to network CSV" : "Exported to local CSV",
                    1
                );
            }

            return Model_Dao_Result.Failure("CSV export failed (both network and local paths)");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error exporting to CSV: {ex.Message}", ex);
            return Model_Dao_Result.Failure($"CSV export error: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> RegenerateLabelCsvAsync(int labelId)
    {
        try
        {
            var labelResult = await GetLabelByIdAsync(labelId);
            if (!labelResult.IsSuccess)
            {
                return Model_Dao_Result.Failure($"Label not found: {labelResult.ErrorMessage}");
            }

            return await ExportLabelToCsvAsync(labelResult.Data);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error regenerating CSV: {ex.Message}", ex);
            return Model_Dao_Result.Failure($"Error regenerating CSV: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> ResetCsvFileAsync()
    {
        try
        {
            var networkPath = _configuration["RoutingModule:CsvExportPath:Network"];
            var localPath = _configuration["RoutingModule:CsvExportPath:Local"];

            bool networkDeleted = false;
            bool localDeleted = false;

            if (File.Exists(networkPath))
            {
                File.Delete(networkPath);
                networkDeleted = true;
            }

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
                localDeleted = true;
            }

            await _logger.LogInformationAsync($"CSV files reset (network: {networkDeleted}, local: {localDeleted})");

            return Model_Dao_Result.Success("CSV files reset successfully", 1);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error resetting CSV: {ex.Message}", ex);
            return Model_Dao_Result.Failure($"Error resetting CSV: {ex.Message}", ex);
        }
    }

    public Model_Dao_Result ValidateLabel(Model_RoutingLabel label)
    {
        if (label == null)
        {
            return Model_Dao_Result.Failure("Label cannot be null");
        }

        if (string.IsNullOrWhiteSpace(label.PONumber))
        {
            return Model_Dao_Result.Failure("PO Number is required");
        }

        if (string.IsNullOrWhiteSpace(label.LineNumber))
        {
            return Model_Dao_Result.Failure("Line Number is required");
        }

        if (label.Quantity <= 0)
        {
            return Model_Dao_Result.Failure("Quantity must be greater than zero");
        }

        if (label.RecipientId <= 0)
        {
            return Model_Dao_Result.Failure("Recipient must be selected");
        }

        if (label.EmployeeNumber <= 0)
        {
            return Model_Dao_Result.Failure("Employee Number is required");
        }

        // Validate that either PO is valid OR an OTHER reason is provided
        if (label.PONumber.Equals("OTHER", StringComparison.OrdinalIgnoreCase))
        {
            if (!label.OtherReasonId.HasValue || label.OtherReasonId.Value <= 0)
            {
                return Model_Dao_Result.Failure("OTHER reason must be selected when PO is OTHER");
            }
        }

        return Model_Dao_Result.Success("Validation passed", 1);
    }

    // Private helper methods

    private string FormatCsvLine(Model_RoutingLabel label)
    {
        // CSV format: PO,Line,Part,Quantity,Recipient,Location,Date
        return $"{label.PONumber},{label.LineNumber},{label.PartID ?? ""},{ label.Quantity},{label.RecipientName ?? ""},{label.RecipientLocation ?? ""},{label.CreatedDate:yyyy-MM-dd HH:mm:ss}";
    }

    private async Task<bool> TryWriteCsvAsync(string filePath, string csvLine, int retryCount, int retryDelayMs)
    {
        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Append to CSV file
                await File.AppendAllTextAsync(filePath, csvLine + Environment.NewLine);
                
                return true;
            }
            catch (IOException ex)
            {
                await _logger.LogWarningAsync($"CSV write attempt {attempt}/{retryCount} failed: {ex.Message}");
                
                if (attempt < retryCount)
                {
                    await Task.Delay(retryDelayMs);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Unexpected CSV write error: {ex.Message}", ex);
                return false;
            }
        }

        return false;
    }

    private List<(string FieldName, string OldValue, string NewValue)> GetLabelChanges(
        Model_RoutingLabel original,
        Model_RoutingLabel updated)
    {
        var changes = new List<(string, string, string)>();

        if (original.PONumber != updated.PONumber)
            changes.Add(("PONumber", original.PONumber, updated.PONumber));

        if (original.LineNumber != updated.LineNumber)
            changes.Add(("LineNumber", original.LineNumber, updated.LineNumber));

        if (original.PartID != updated.PartID)
            changes.Add(("PartID", original.PartID ?? "", updated.PartID ?? ""));

        if (original.Quantity != updated.Quantity)
            changes.Add(("Quantity", original.Quantity.ToString(), updated.Quantity.ToString()));

        if (original.RecipientId != updated.RecipientId)
            changes.Add(("RecipientId", original.RecipientId.ToString(), updated.RecipientId.ToString()));

        if (original.OtherReasonId != updated.OtherReasonId)
            changes.Add(("OtherReasonId", 
                original.OtherReasonId?.ToString() ?? "null", 
                updated.OtherReasonId?.ToString() ?? "null"));

        return changes;
    }
}
