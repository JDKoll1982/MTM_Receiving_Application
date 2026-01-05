using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for managing routing label entry, queue, and CSV export.
/// Handles label creation, numbering, and LabelView-compatible CSV generation.
/// </summary>
public class Service_Routing : IService_Routing
{
    private readonly Dao_Routing_Label _daoLabel;
    private readonly Dao_Routing_Recipient _daoRecipient;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_LoggingUtility _logger;
    private readonly string _localCSVPath;

    public Service_Routing(
        Dao_Routing_Label daoLabel,
        Dao_Routing_Recipient daoRecipient,
        IService_UserSessionManager sessionManager,
        IService_LoggingUtility logger)
    {
        _daoLabel = daoLabel ?? throw new ArgumentNullException(nameof(daoLabel));
        _daoRecipient = daoRecipient ?? throw new ArgumentNullException(nameof(daoRecipient));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Setup local CSV path
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = Path.Combine(appDataPath, "MTM_Receiving_Application", "Routing");
        if (!Directory.Exists(appDir))
        {
            Directory.CreateDirectory(appDir);
        }
        _localCSVPath = Path.Combine(appDir, "RoutingLabels.csv");
    }

    public async Task<Model_Dao_Result<int>> AddLabelAsync(Model_Routing_Label label)
    {
        try
        {
            _logger.LogInfo($"Adding routing label: {label.DeliverTo} -> {label.Department}");

            // Validate label data
            if (string.IsNullOrWhiteSpace(label.DeliverTo))
            {
                return Model_Dao_Result_Factory.Failure<int>("Deliver To recipient is required");
            }

            if (string.IsNullOrWhiteSpace(label.Department))
            {
                return Model_Dao_Result_Factory.Failure<int>("Department is required");
            }

            if (string.IsNullOrWhiteSpace(label.EmployeeNumber))
            {
                return Model_Dao_Result_Factory.Failure<int>("Employee number is required");
            }

            // If label number not set, get next number
            if (label.LabelNumber <= 0)
            {
                var nextNumberResult = await GetNextLabelNumberAsync();
                if (!nextNumberResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(nextNumberResult.ErrorMessage);
                }
                label.LabelNumber = nextNumberResult.Data;
            }

            // Set created date if not set
            if (label.CreatedDate == default)
            {
                label.CreatedDate = DateTime.Today;
            }

            // Example logic to add
            if (!string.IsNullOrWhiteSpace(label.PoNumber) &&
                int.TryParse(label.PoNumber, out _) &&
                label.PoNumber.Length == 5)
            {
                label.PoNumber = $"PO-{label.PoNumber}";
            }

            // Insert label
            return await _daoLabel.InsertAsync(label);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding routing label: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error adding label: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_Routing_Label>>> GetTodayLabelsAsync()
    {
        try
        {
            _logger.LogInfo("Retrieving today's routing labels");
            return await _daoLabel.GetTodayLabelsAsync(DateTime.Today);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving today's labels: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Routing_Label>>($"Error retrieving labels: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> DuplicateLabelAsync(int labelId)
    {
        try
        {
            _logger.LogInfo($"Duplicating routing label ID: {labelId}");

            // Get today's labels to find the one to duplicate
            var todayLabelsResult = await GetTodayLabelsAsync();
            if (!todayLabelsResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(todayLabelsResult.ErrorMessage);
            }

            var originalLabel = (todayLabelsResult.Data ?? new List<Model_Routing_Label>()).FirstOrDefault(l => l.Id == labelId);
            if (originalLabel == null)
            {
                return Model_Dao_Result_Factory.Failure<int>("Label not found in today's queue");
            }

            // Get next label number
            var nextNumberResult = await GetNextLabelNumberAsync();
            if (!nextNumberResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(nextNumberResult.ErrorMessage);
            }

            // Create duplicate with new label number
            var duplicate = new Model_Routing_Label
            {
                LabelNumber = nextNumberResult.Data,
                DeliverTo = originalLabel.DeliverTo,
                Department = originalLabel.Department,
                PackageDescription = originalLabel.PackageDescription,
                PoNumber = originalLabel.PoNumber,
                WorkOrder = originalLabel.WorkOrder,
                EmployeeNumber = originalLabel.EmployeeNumber,
                CreatedDate = DateTime.Today
            };

            return await _daoLabel.InsertAsync(duplicate);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error duplicating label: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error duplicating label: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> GetNextLabelNumberAsync()
    {
        try
        {
            // Get today's labels and find max label number
            var todayLabelsResult = await _daoLabel.GetTodayLabelsAsync(DateTime.Today);
            if (!todayLabelsResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(todayLabelsResult.ErrorMessage);
            }

            var maxLabelNumber = 0;
            if (todayLabelsResult.Data?.Count > 0)
            {
                maxLabelNumber = todayLabelsResult.Data.Max(l => l.LabelNumber);
            }

            var nextNumber = maxLabelNumber + 1;
            _logger.LogInfo($"Next label number: {nextNumber}");
            return Model_Dao_Result_Factory.Success<int>(nextNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting next label number: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error getting next label number: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<string>> ExportToCSVAsync(List<Model_Routing_Label> labels, string? filePath = null)
    {
        try
        {
            if (labels == null || labels.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<string>("No labels to export");
            }

            var targetPath = filePath ?? _localCSVPath;
            _logger.LogInfo($"Exporting {labels.Count} labels to CSV: {targetPath}");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write CSV file compatible with LabelView 2022
            await using var writer = new StreamWriter(targetPath);
            await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            // Write header
            csv.WriteField("LabelNumber");
            csv.WriteField("DeliverTo");
            csv.WriteField("Department");
            csv.WriteField("PackageDescription");
            csv.WriteField("PONumber");
            csv.WriteField("WorkOrder");
            csv.WriteField("EmployeeNumber");
            csv.WriteField("Date");
            await csv.NextRecordAsync();

            // Write data rows
            foreach (var label in labels)
            {
                csv.WriteField(label.LabelNumber);
                csv.WriteField(label.DeliverTo);
                csv.WriteField(label.Department);
                csv.WriteField(label.PackageDescription ?? string.Empty);
                csv.WriteField(label.PoNumber ?? string.Empty);
                csv.WriteField(label.WorkOrder ?? string.Empty);
                csv.WriteField(label.EmployeeNumber);
                csv.WriteField(label.CreatedDate.ToString("yyyy-MM-dd"));
                await csv.NextRecordAsync();
            }

            _logger.LogInfo($"CSV export completed: {targetPath}");
            return Model_Dao_Result_Factory.Success(targetPath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Error exporting CSV: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> UpdateLabelAsync(Model_Routing_Label label)
    {
        try
        {
            _logger.LogInfo($"Updating routing label ID: {label.Id}");

            // Validate label data
            if (string.IsNullOrWhiteSpace(label.DeliverTo))
            {
                return Model_Dao_Result_Factory.Failure("Deliver To recipient is required");
            }

            if (string.IsNullOrWhiteSpace(label.Department))
            {
                return Model_Dao_Result_Factory.Failure("Department is required");
            }

            return await _daoLabel.UpdateAsync(label);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating routing label: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating label: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> DeleteLabelAsync(int labelId)
    {
        try
        {
            _logger.LogInfo($"Deleting routing label ID: {labelId}");
            return await _daoLabel.DeleteAsync(labelId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting routing label: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deleting label: {ex.Message}", ex);
        }
    }
}
