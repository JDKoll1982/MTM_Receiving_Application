using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
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

    public string NormalizePONumber(string? poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return "No PO";
        }

        poNumber = poNumber.Trim();

        if (poNumber.Equals("Customer Supplied", StringComparison.OrdinalIgnoreCase))
        {
            return "Customer Supplied";
        }

        string numericPart = new string(poNumber.TakeWhile(char.IsDigit).ToArray());
        string suffix = poNumber.Substring(numericPart.Length);

        if (numericPart.Length < 5)
        {
            return "Validate PO";
        }

        if (numericPart.Length == 5)
        {
            numericPart = "0" + numericPart;
        }

        return "PO-" + numericPart + suffix;
    }

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
            var moduleName = data[0].SourceModule?.Trim().ToLowerInvariant() ?? string.Empty;

            html.AppendLine("<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");
            html.AppendLine("<table style='border-collapse: collapse; width: 100%; table-layout: fixed;'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr style='background-color: #d9e8fb; font-weight: 700;'>");

            switch (moduleName)
            {
                case "receiving":
                    AppendHeaderCell(html, "PO Number", "18%");
                    AppendHeaderCell(html, "Part", "16%");
                    AppendHeaderCell(html, "Description", "24%");
                    AppendHeaderCell(html, "Qty", "10%");
                    AppendHeaderCell(html, "Weight", "10%");
                    AppendHeaderCell(html, "Heat/Lot", "12%");
                    AppendHeaderCell(html, "Date", "10%");
                    break;

                case "dunnage":
                    AppendHeaderCell(html, "Type", "18%");
                    AppendHeaderCell(html, "Part", "18%");
                    AppendHeaderCell(html, "Specs", "34%");
                    AppendHeaderCell(html, "Qty", "10%");
                    AppendHeaderCell(html, "Date", "20%");
                    break;

                case "volvo":
                    AppendHeaderCell(html, "Shipment #", "16%");
                    AppendHeaderCell(html, "PO Number", "22%");
                    AppendHeaderCell(html, "Receiver #", "18%");
                    AppendHeaderCell(html, "Status", "16%");
                    AppendHeaderCell(html, "Part Count", "12%");
                    AppendHeaderCell(html, "Date", "16%");
                    break;

                default:
                    AppendHeaderCell(html, "Data", "100%");
                    break;
            }

            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            var currentDate = DateTime.MinValue;
            bool isLightRow = true;

            foreach (var row in data)
            {
                if (applyDateGrouping && row.CreatedDate.Date != currentDate.Date)
                {
                    currentDate = row.CreatedDate.Date;
                    isLightRow = !isLightRow;
                }

                var bgColor = isLightRow ? "#ffffff" : "#f4f8fc";
                html.AppendLine($"<tr style='background-color: {bgColor};'>");

                switch (moduleName)
                {
                    case "receiving":
                        AppendBodyCell(html, row.PONumber);
                        AppendBodyCell(html, row.PartNumber);
                        AppendBodyCell(html, row.PartDescription);
                        AppendBodyCell(html, FormatDecimal(row.Quantity), "right");
                        AppendBodyCell(html, FormatDecimal(row.WeightLbs), "right");
                        AppendBodyCell(html, row.HeatLotNumber);
                        AppendBodyCell(html, row.CreatedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        break;

                    case "dunnage":
                        AppendBodyCell(html, row.DunnageType);
                        AppendBodyCell(html, row.PartNumber);
                        AppendBodyCell(html, row.SpecsCombined);
                        AppendBodyCell(html, FormatDecimal(row.Quantity), "right");
                        AppendBodyCell(html, row.CreatedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        break;

                    case "volvo":
                        AppendBodyCell(html, row.ShipmentNumber?.ToString(CultureInfo.InvariantCulture));
                        AppendBodyCell(html, row.PONumber);
                        AppendBodyCell(html, row.ReceiverNumber);
                        AppendBodyCell(html, row.Status);
                        AppendBodyCell(html, row.PartCount?.ToString(CultureInfo.InvariantCulture), "right");
                        AppendBodyCell(html, row.CreatedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        break;

                    default:
                        AppendBodyCell(html, row.SourceModule);
                        break;
                }

                html.AppendLine("</tr>");

                if (!applyDateGrouping)
                {
                    isLightRow = !isLightRow;
                }
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            return Model_Dao_Result_Factory.Success(html.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error formatting email: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Email formatting failed: {ex.Message}", ex);
        }
    }

    private static void AppendHeaderCell(StringBuilder html, string text, string width)
    {
        html.AppendLine($"<th style='border: 1px solid #9fbad0; padding: 8px 10px; text-align: left; width: {width}; white-space: nowrap;'>{text}</th>");
    }

    private static void AppendBodyCell(StringBuilder html, string? value, string alignment = "left")
    {
        var safeValue = System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        html.AppendLine($"<td style='border: 1px solid #c8d6e5; padding: 6px 10px; text-align: {alignment}; vertical-align: top; word-wrap: break-word;'>{safeValue}</td>");
    }

    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
