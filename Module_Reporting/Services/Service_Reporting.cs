using System;
using System.Collections.Generic;
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
/// Handles data retrieval, PO normalization, and email formatting
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
