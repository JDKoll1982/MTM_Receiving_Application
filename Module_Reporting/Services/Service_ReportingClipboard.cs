using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Reporting.Services;

public class Service_ReportingClipboard : IService_ReportingClipboard
{
    public Model_Dao_Result<DataPackage> CreateClipboardPackage(List<Model_ReportRow> rows, string htmlFragment)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentException.ThrowIfNullOrWhiteSpace(htmlFragment);

            var dataPackage = new DataPackage();
            var htmlFormat = HtmlFormatHelper.CreateHtmlFormat(htmlFragment);

            dataPackage.SetHtmlFormat(htmlFormat);
            dataPackage.SetText(BuildPlainTextFallback(rows));

            return Model_Dao_Result_Factory.Success(dataPackage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<DataPackage>($"Failed to create clipboard package: {ex.Message}", ex);
        }
    }

    private static string BuildPlainTextFallback(List<Model_ReportRow> rows)
    {
        if (rows.Count == 0)
        {
            return "No data to display";
        }

        var text = new StringBuilder();

        text.AppendLine("PO\tPart/Dunnage\tQuantity\tLocation\tNotes\tLoads/Skids\tCoils/Pcs/Type per Skid");
        foreach (var row in rows)
        {
            text.AppendLine($"{row.DisplayPo}\t{row.DisplayPartOrDunnage}\t{row.DisplayQuantity}\t{row.DisplayLocation}\t{row.DisplayNotes}\t{row.DisplayLoadsOrSkids}\t{row.DisplayUnitsPerSkid}");
        }

        return text.ToString();
    }
}