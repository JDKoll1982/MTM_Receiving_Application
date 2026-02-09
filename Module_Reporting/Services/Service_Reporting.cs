using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Data;

namespace MTM_Receiving_Application.Module_Reporting.Services;

/// <summary>
/// Service implementation for End-of-Day reporting
/// Handles data retrieval, PO normalization, CSV export, and email formatting
/// </summary>
public class Service_Reporting : IService_Reporting
{
    private readonly Dao_Reporting _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_Reporting(
        Dao_Reporting dao,
        IService_LoggingUtility logger)
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInfo($"Retrieving Receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        var result = await _dao.GetReceivingHistoryAsync(startDate, endDate);

        // Normalize PO numbers
        if (result.IsSuccess && result.Data != null)
        {
            foreach (var row in result.Data)
            {
                row.PONumber = NormalizePONumber(row.PONumber);
            }
        }

        return result;
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInfo($"Retrieving Dunnage history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        return await _dao.GetDunnageHistoryAsync(startDate, endDate);
    }

    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInfo($"Retrieving Volvo history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        return await _dao.GetVolvoHistoryAsync(startDate, endDate);
    }

    public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInfo($"Checking module availability from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        return await _dao.CheckAvailabilityAsync(startDate, endDate);
    }

    /// <summary>
    /// Normalizes PO number to standard format
    /// Matches EndOfDayEmail.js normalizePO() algorithm
    /// </summary>
    /// <param name="poNumber"></param>
    public string NormalizePONumber(string? poNumber)
    {
        // Handle null or empty
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return "No PO";
        }

        poNumber = poNumber.Trim();

        // Pass through specific non-numeric values
        if (poNumber.Equals("Customer Supplied", StringComparison.OrdinalIgnoreCase))
        {
            return "Customer Supplied";
        }

        // Extract numeric part and suffix
        string numericPart = new string(poNumber.TakeWhile(char.IsDigit).ToArray());
        string suffix = poNumber.Substring(numericPart.Length);

        // Validate length (must be at least 5 digits)
        if (numericPart.Length < 5)
        {
            return "Validate PO";
        }

        // Pad to 6 digits if exactly 5
        if (numericPart.Length == 5)
        {
            numericPart = "0" + numericPart;
        }

        // Format as PO-XXXXXX + optional suffix
        return "PO-" + numericPart + suffix;
    }

    /// <summary>
    /// Exports data to CSV file matching MiniUPSLabel.csv structure
    /// </summary>
    /// <param name="data"></param>
    /// <param name="moduleName"></param>
    public async Task<Model_Dao_Result<string>> ExportToCSVAsync(
        List<Model_ReportRow> data,
        string moduleName)
    {
        try
        {
            _logger.LogInfo($"Exporting {data.Count} records to CSV for {moduleName} module");

            // Create CSV content based on module
            var csv = new StringBuilder();

            switch (moduleName.ToLower())
            {
                case "receiving":
                    csv.AppendLine("PO Number,Part,Description,Qty,Weight,Heat/Lot,Date");
                    foreach (var row in data)
                    {
                        csv.AppendLine($"\"{row.PONumber ?? ""}\",\"{row.PartNumber ?? ""}\",\"{row.PartDescription ?? ""}\",{row.Quantity ?? 0},{row.WeightLbs ?? 0},\"{row.HeatLotNumber ?? ""}\",{row.CreatedDate:yyyy-MM-dd}");
                    }
                    break;

                case "dunnage":
                    csv.AppendLine("Type,Part,Specs,Qty,Date");
                    foreach (var row in data)
                    {
                        csv.AppendLine($"\"{row.DunnageType ?? ""}\",\"{row.PartNumber ?? ""}\",\"{row.SpecsCombined ?? ""}\",{row.Quantity ?? 0},{row.CreatedDate:yyyy-MM-dd}");
                    }
                    break;

                case "routing":
                    csv.AppendLine("Deliver To,Department,Package Description,PO Number,Work Order,Date");
                    foreach (var row in data)
                    {
                        csv.AppendLine($"\"{row.DeliverTo ?? ""}\",\"{row.Department ?? ""}\",\"{row.PackageDescription ?? ""}\",\"{row.PONumber ?? ""}\",\"{row.WorkOrderNumber ?? ""}\",{row.CreatedDate:yyyy-MM-dd}");
                    }
                    break;

                case "volvo":
                    csv.AppendLine("Shipment Number,PO Number,Receiver Number,Status,Date,Part Count");
                    foreach (var row in data)
                    {
                        csv.AppendLine($"{row.ShipmentNumber ?? 0},\"{row.PONumber ?? ""}\",\"{row.ReceiverNumber ?? ""}\",\"{row.Status ?? ""}\",{row.CreatedDate:yyyy-MM-dd},{row.PartCount ?? 0}");
                    }
                    break;

                default:
                    return Model_Dao_Result_Factory.Failure<string>("Unknown module name for CSV export");
            }

            // Save to file
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"EoD_{moduleName}_{timestamp}.csv";
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var mtmFolder = Path.Combine(appDataPath, "MTM_Receiving_Application", "Reports");
            Directory.CreateDirectory(mtmFolder);
            var filePath = Path.Combine(mtmFolder, fileName);

            await File.WriteAllTextAsync(filePath, csv.ToString());
            _logger.LogInfo($"CSV exported successfully to {filePath}");

            return Model_Dao_Result_Factory.Success(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"CSV export failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Formats data as HTML table with alternating row colors grouped by date
    /// Matches Google Sheets colorHistory() function behavior
    /// </summary>
    /// <param name="data"></param>
    /// <param name="applyDateGrouping"></param>
    public async Task<Model_Dao_Result<string>> FormatForEmailAsync(
        List<Model_ReportRow> data,
        bool applyDateGrouping = true)
    {
        try
        {
            _logger.LogInfo($"Formatting {data.Count} records for email");

            if (data.Count == 0)
            {
                return Model_Dao_Result_Factory.Success("<p>No data to display</p>");
            }

            var html = new StringBuilder();
            html.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");

            // Determine module type from first row
            var moduleName = data[0].SourceModule;

            // Add header based on module type
            html.AppendLine("<thead>");
            html.AppendLine("<tr style='background-color: #f0f0f0; font-weight: bold;'>");

            switch (moduleName.ToLower())
            {
                case "receiving":
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>PO Number</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Part</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Description</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Qty</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Weight</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Heat/Lot</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Date</th>");
                    break;

                case "dunnage":
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Type</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Part</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Specs</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Qty</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Date</th>");
                    break;

                case "routing":
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Deliver To</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Department</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Package Description</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>PO Number</th>");
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Date</th>");
                    break;

                default:
                    html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px;'>Data</th>");
                    break;
            }

            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            // Add rows with alternating colors (by date if grouping enabled)
            var currentDate = DateTime.MinValue;
            bool isLightRow = true;

            foreach (var row in data)
            {
                // Toggle color when date changes (if grouping enabled)
                if (applyDateGrouping && row.CreatedDate.Date != currentDate.Date)
                {
                    currentDate = row.CreatedDate.Date;
                    isLightRow = !isLightRow;
                }

                var bgColor = isLightRow ? "#ffffff" : "#f9f9f9";
                html.AppendLine($"<tr style='background-color: {bgColor};'>");

                switch (moduleName.ToLower())
                {
                    case "receiving":
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PONumber ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PartNumber ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PartDescription ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.Quantity ?? 0}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.WeightLbs ?? 0}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.HeatLotNumber ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.CreatedDate:yyyy-MM-dd}</td>");
                        break;

                    case "dunnage":
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.DunnageType ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PartNumber ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.SpecsCombined ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.Quantity ?? 0}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.CreatedDate:yyyy-MM-dd}</td>");
                        break;

                    case "routing":
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.DeliverTo ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.Department ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PackageDescription ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.PONumber ?? ""}</td>");
                        html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{row.CreatedDate:yyyy-MM-dd}</td>");
                        break;
                }

                html.AppendLine("</tr>");

                // Toggle for next row if not grouping by date
                if (!applyDateGrouping)
                {
                    isLightRow = !isLightRow;
                }
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            return Model_Dao_Result_Factory.Success(html.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error formatting email: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Email formatting failed: {ex.Message}", ex);
        }
    }
}
