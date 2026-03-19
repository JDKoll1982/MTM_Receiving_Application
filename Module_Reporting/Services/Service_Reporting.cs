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
            return string.Empty;
        }

        poNumber = poNumber.Trim();

        if (poNumber.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
        {
            return poNumber;
        }

        if (poNumber.Equals("Customer Supplied", StringComparison.OrdinalIgnoreCase))
        {
            return "Customer Supplied";
        }

        if (!char.IsDigit(poNumber[0]))
        {
            return poNumber;
        }

        string numericPart = new string(poNumber.TakeWhile(char.IsDigit).ToArray());
        string suffix = poNumber.Substring(numericPart.Length);

        if (numericPart.Length < 5)
        {
            return poNumber;
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

            html.AppendLine("<div style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");
            html.AppendLine("<table style='border-collapse: collapse; width: 100%; table-layout: fixed;'>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr style='background-color: #d9e8fb; font-weight: 700;'>");

            AppendHeaderCell(html, "PO", "14%");
            AppendHeaderCell(html, "Part/Dunnage", "22%");
            AppendHeaderCell(html, "Quantity", "10%");
            AppendHeaderCell(html, "Location", "14%");
            AppendHeaderCell(html, "Notes", "20%");
            AppendHeaderCell(html, "Loads/Skids", "10%");
            AppendHeaderCell(html, "Coils/Pcs/Type per Skid", "10%");

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

                AppendBodyCell(html, row.DisplayPo);
                AppendBodyCell(html, row.DisplayPartOrDunnage);
                AppendBodyCell(html, row.DisplayQuantity, "right");
                AppendBodyCell(html, row.DisplayLocation);
                AppendBodyCell(html, row.DisplayNotes);
                AppendBodyCell(html, row.DisplayLoadsOrSkids, "right");
                AppendBodyCell(html, row.DisplayUnitsPerSkid);

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

}
