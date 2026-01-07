using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Constants;
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
    private readonly IRoutingRecipientService _recipientService;
    private readonly IService_LoggingUtility _logger;
    private readonly IConfiguration _configuration;

    // Static semaphore for CSV file write synchronization (fixes Issue #3: race condition)
    private static readonly SemaphoreSlim _csvFileLock = new SemaphoreSlim(1, 1);

    public RoutingService(
        Dao_RoutingLabel daoLabel,
        Dao_RoutingLabelHistory daoHistory,
        IRoutingInforVisualService inforVisualService,
        IRoutingUsageTrackingService usageTrackingService,
        IRoutingRecipientService recipientService,
        IService_LoggingUtility logger,
        IConfiguration configuration)
    {
        _daoLabel = daoLabel ?? throw new ArgumentNullException(nameof(daoLabel));
        _daoHistory = daoHistory ?? throw new ArgumentNullException(nameof(daoHistory));
        _inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        _usageTrackingService = usageTrackingService ?? throw new ArgumentNullException(nameof(usageTrackingService));
        _recipientService = recipientService ?? throw new ArgumentNullException(nameof(recipientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Creates a new routing label with validation, CSV export, and usage tracking
    /// Issue #20: Refactored for clarity - extracted validation and background tasks
    /// </summary>
    /// <param name="label">Label to create</param>
    /// <returns>Result with new label ID</returns>
    public async Task<Model_Dao_Result<int>> CreateLabelAsync(Model_RoutingLabel label)
    {
        try
        {
            await _logger.LogInfoAsync($"Creating routing label for PO {label.PONumber}, Recipient {label.RecipientId}");

            // Step 1: Validate label and check for duplicates
            var validationResult = await ValidateAndCheckDuplicatesAsync(label);
            if (!validationResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(validationResult.ErrorMessage);
            }

            // Step 2: Insert label into database
            var insertResult = await _daoLabel.InsertLabelAsync(label);
            if (!insertResult.IsSuccess)
            {
                return insertResult;
            }

            int labelId = insertResult.Data;
            label.Id = labelId;

            // Step 3: Execute non-critical background tasks (CSV export, usage tracking)
            ExecuteBackgroundTasks(label, labelId);

            return Model_Dao_Result_Factory.Success<int>(labelId);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error creating label: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error creating label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Issue #20: Validates label and checks for duplicates (extracted from CreateLabelAsync)
    /// </summary>
    private async Task<Model_Dao_Result> ValidateAndCheckDuplicatesAsync(Model_RoutingLabel label)
    {
        // Basic validation
        var validationResult = ValidateLabel(label);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        // Issue #9: Validate recipient exists
        var recipientValidation = await _recipientService.ValidateRecipientExistsAsync(label.RecipientId);
        if (!recipientValidation.IsSuccess || !recipientValidation.Data)
        {
            return Model_Dao_Result_Factory.Failure($"Invalid recipient ID: {label.RecipientId}");
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
            return Model_Dao_Result_Factory.Failure(
                $"Duplicate label exists (ID: {duplicateResult.Data.ExistingLabelId})"
            );
        }

        return Model_Dao_Result_Factory.Success();
    }

    /// <summary>
    /// Issue #20: Executes CSV export and usage tracking in background (extracted from CreateLabelAsync)
    /// </summary>
    private void ExecuteBackgroundTasks(Model_RoutingLabel label, int labelId)
    {
        // Issue #2: CSV export and usage tracking are non-critical operations
        _ = Task.Run(async () =>
        {
            try
            {
                var csvResult = await ExportLabelToCsvAsync(label);
                if (!csvResult.IsSuccess)
                {
                    await _logger.LogWarningAsync($"CSV export failed for label {labelId}: {csvResult.ErrorMessage}");
                }

                var usageResult = await _usageTrackingService.IncrementUsageCountAsync(
                    label.CreatedBy,
                    label.RecipientId
                );
                if (!usageResult.IsSuccess)
                {
                    await _logger.LogWarningAsync($"Usage tracking failed for label {labelId}: {usageResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Background task error for label {labelId}: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Updates an existing label and logs changes to history
    /// </summary>
    /// <param name="label">Updated label data</param>
    /// <param name="editedByEmployeeNumber">Employee number making the edit</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label, int editedByEmployeeNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Updating label {label.Id} by employee {editedByEmployeeNumber}");

            // Get original label for history tracking
            var originalResult = await _daoLabel.GetLabelByIdAsync(label.Id);
            if (!originalResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure($"Original label not found: {originalResult.ErrorMessage}");
            }

            var original = originalResult.Data;

            // Validate updated label
            var validationResult = ValidateLabel(label);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // Update label
            var updateResult = await _daoLabel.UpdateLabelAsync(label);
            if (!updateResult.IsSuccess)
            {
                return updateResult;
            }

            // Log changes to history
            if (original != null)
            {
                var changes = GetLabelChanges(original, label);

                // Issue #18: Use batch insert to avoid N+1 query problem
                var historyEntries = new List<Model_RoutingLabelHistory>();
                foreach (var (fieldName, oldValue, newValue) in changes)
                {
                    historyEntries.Add(new Model_RoutingLabelHistory
                    {
                        LabelId = label.Id,
                        FieldChanged = fieldName,
                        OldValue = oldValue,
                        NewValue = newValue,
                        EditedBy = editedByEmployeeNumber,
                        EditDate = DateTime.Now
                    });
                }

                if (historyEntries.Count > 0)
                {
                    var historyResult = await _daoHistory.InsertHistoryBatchAsync(historyEntries);
                    if (!historyResult.IsSuccess)
                    {
                        await _logger.LogWarningAsync($"Failed to log history: {historyResult.ErrorMessage}");
                    }
                }
            }

            return Model_Dao_Result_Factory.Success("Label updated successfully", 1);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error updating label: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a specific label by ID
    /// </summary>
    /// <param name="labelId">Label ID to retrieve</param>
    /// <returns>Result with label data or error</returns>
    public async Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId)
    {
        try
        {
            return await _daoLabel.GetLabelByIdAsync(labelId);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting label {labelId}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_RoutingLabel>($"Error getting label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all labels with optional pagination
    /// </summary>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="offset">Number of records to skip</param>
    /// <returns>Result with list of labels</returns>
    public async Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0)
    {
        try
        {
            // Issue #13: Pagination support requires DAO updates
            // Would need: sp_routing_label_search_paginated(p_search, p_limit, p_offset)
            // Returns: Subset of results + total count for UI paging controls
            // Priority: MEDIUM - Useful for large datasets (1000+ labels)
            // Current implementation returns all matching labels (acceptable for <500 records)
            // Current: DAO method accepts params but doesn't implement pagination
            // Impact: Performance issue when loading large label sets
            return await _daoLabel.GetAllLabelsAsync(limit, offset);
        }
        catch (Exception ex)
        {
            // Issue #11: Standardized error handling pattern
            await _logger.LogErrorAsync($"Error retrieving labels: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingLabel>>($"Failed to retrieve labels: {ex.Message}", ex);
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
                return result;
            }

            return Model_Dao_Result_Factory.Failure<(bool Exists, int? ExistingLabelId)>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error checking duplicate: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<(bool Exists, int? ExistingLabelId)>($"Error checking duplicate: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exports a label to CSV file with retry logic and fallback paths
    /// </summary>
    /// <param name="label">Label to export</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> ExportLabelToCsvAsync(Model_RoutingLabel label)
    {
        try
        {
            // Get CSV export paths from configuration
            var networkPath = _configuration[Constant_RoutingConfiguration.CsvExportPathNetwork];
            var localPath = _configuration[Constant_RoutingConfiguration.CsvExportPathLocal];
            var retryCount = _configuration.GetValue<int>(
                Constant_RoutingConfiguration.CsvRetryMaxAttempts,
                Constant_RoutingConfiguration.DefaultCsvRetryMaxAttempts);
            var retryDelay = _configuration.GetValue<int>(
                Constant_RoutingConfiguration.CsvRetryDelayMs,
                Constant_RoutingConfiguration.DefaultCsvRetryDelayMs);

            // Format CSV line
            var csvLine = FormatCsvLine(label);

            // Validate and sanitize paths (Issue #5)
            var validatedNetworkPath = string.IsNullOrEmpty(networkPath) ? string.Empty : ValidateCsvPath(networkPath);
            var validatedLocalPath = string.IsNullOrEmpty(localPath) ? string.Empty : ValidateCsvPath(localPath);

            // Try network path first with retry
            bool networkSuccess = await TryWriteCsvAsync(validatedNetworkPath, csvLine, retryCount, retryDelay);

            // Try local path as fallback
            bool localSuccess = false;
            if (!networkSuccess)
            {
                await _logger.LogWarningAsync($"Network CSV export failed, using local path: {validatedLocalPath}");
                localSuccess = await TryWriteCsvAsync(validatedLocalPath, csvLine, retryCount, retryDelay);
            }

            if (networkSuccess || localSuccess)
            {
                // Mark as exported in database
                await _daoLabel.MarkExportedAsync(new List<int> { label.Id });

                return Model_Dao_Result_Factory.Success(
                    networkSuccess ? "Exported to network CSV" : "Exported to local CSV",
                    1
                );
            }

            return Model_Dao_Result_Factory.Failure("CSV export failed (both network and local paths)");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error exporting to CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"CSV export error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Regenerates CSV export for a specific label (e.g., after export failure)
    /// </summary>
    /// <param name="labelId">Label ID to regenerate</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> RegenerateLabelCsvAsync(int labelId)
    {
        try
        {
            var labelResult = await GetLabelByIdAsync(labelId);
            if (!labelResult.IsSuccess || labelResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure($"Label not found: {labelResult.ErrorMessage}");
            }

            return await ExportLabelToCsvAsync(labelResult.Data);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error regenerating CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error regenerating CSV: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Resets CSV exported flag for all labels (administrative operation)
    /// </summary>
    /// <returns>Result indicating success or failure</returns>
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

            await _logger.LogInfoAsync($"CSV files reset (network: {networkDeleted}, local: {localDeleted})");

            return Model_Dao_Result_Factory.Success("CSV files reset successfully", 1);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error resetting CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error resetting CSV: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates label data before database operations
    /// </summary>
    /// <param name="label">Label to validate</param>
    /// <returns>Result indicating if validation passed</returns>
    public Model_Dao_Result ValidateLabel(Model_RoutingLabel label)
    {
        if (label == null)
        {
            return Model_Dao_Result_Factory.Failure("Label cannot be null");
        }

        if (string.IsNullOrWhiteSpace(label.PONumber))
        {
            return Model_Dao_Result_Factory.Failure("PO Number is required");
        }

        // Issue #6: String length validation
        if (label.PONumber.Length > 50)
        {
            return Model_Dao_Result_Factory.Failure("PO Number cannot exceed 50 characters");
        }

        if (string.IsNullOrWhiteSpace(label.LineNumber))
        {
            return Model_Dao_Result_Factory.Failure("Line Number is required");
        }

        if (label.LineNumber.Length > 20)
        {
            return Model_Dao_Result_Factory.Failure("Line Number cannot exceed 20 characters");
        }

        if (label.Description != null && label.Description.Length > 255)
        {
            return Model_Dao_Result_Factory.Failure("Description cannot exceed 255 characters");
        }

        if (label.Quantity <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Quantity must be greater than zero");
        }

        if (label.RecipientId <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Recipient must be selected");
        }

        if (label.CreatedBy <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Employee Number is required");
        }

        // Validate that either PO is valid OR an OTHER reason is provided
        if (label.PONumber.Equals("OTHER", StringComparison.OrdinalIgnoreCase))
        {
            if (!label.OtherReasonId.HasValue || label.OtherReasonId.Value <= 0)
            {
                return Model_Dao_Result_Factory.Failure("OTHER reason must be selected when PO is OTHER");
            }
        }

        return Model_Dao_Result_Factory.Success("Validation passed", 1);
    }

    // Private helper methods

    /// <summary>
    /// Validates CSV file path to prevent directory traversal attacks
    /// </summary>
    /// <param name="path">Path to validate</param>
    /// <returns>Validated full path</returns>
    /// <exception cref="ArgumentException">If path is invalid</exception>
    /// <exception cref="SecurityException">If path contains traversal attempts</exception>
    private string ValidateCsvPath(string path)
    {
        // Issue #5: Path traversal validation
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("CSV path cannot be empty", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);

        // Ensure path doesn't contain traversal attempts
        if (path.Contains("..") || path.Contains("~"))
        {
            throw new System.Security.SecurityException("CSV path contains invalid characters");
        }

        return fullPath;
    }

    /// <summary>
    /// Formats label data as CSV line
    /// </summary>
    /// <param name="label">Label to format</param>
    /// <returns>CSV-formatted string</returns>
    private string FormatCsvLine(Model_RoutingLabel label)
    {
        // CSV format: PO,Line,Part,Quantity,Recipient,Location,Date
        return $"{label.PONumber},{label.LineNumber},{label.Description ?? ""},{label.Quantity},{label.RecipientName ?? ""},{label.RecipientLocation ?? ""},{label.CreatedDate:yyyy-MM-dd HH:mm:ss}";
    }

    private async Task<bool> TryWriteCsvAsync(string filePath, string csvLine, int retryCount, int retryDelayMs)
    {
        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                // Acquire lock to prevent concurrent CSV writes (fixes Issue #3)
                await _csvFileLock.WaitAsync();

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
                finally
                {
                    _csvFileLock.Release();
                }
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

    /// <summary>
    /// Compares two labels and returns list of changed fields
    /// </summary>
    /// <param name="original">Original label</param>
    /// <param name="updated">Updated label</param>
    /// <returns>List of tuples containing field name, old value, and new value</returns>
    private List<(string FieldName, string OldValue, string NewValue)> GetLabelChanges(
        Model_RoutingLabel original,
        Model_RoutingLabel updated)
    {
        var changes = new List<(string, string, string)>();

        if (original.PONumber != updated.PONumber)
        {
            changes.Add(("PONumber", original.PONumber, updated.PONumber));
        }

        if (original.LineNumber != updated.LineNumber)
        {
            changes.Add(("LineNumber", original.LineNumber, updated.LineNumber));
        }

        if (original.Description != updated.Description)
        {
            changes.Add(("Description", original.Description ?? "", updated.Description ?? ""));
        }

        if (original.Quantity != updated.Quantity)
        {
            changes.Add(("Quantity", original.Quantity.ToString(), updated.Quantity.ToString()));
        }

        if (original.RecipientId != updated.RecipientId)
        {
            changes.Add(("RecipientId", original.RecipientId.ToString(), updated.RecipientId.ToString()));
        }

        if (original.OtherReasonId != updated.OtherReasonId)
        {
            changes.Add(("OtherReasonId",
                original.OtherReasonId?.ToString() ?? "null",
                updated.OtherReasonId?.ToString() ?? "null"));
        }

        return changes;
    }
}
