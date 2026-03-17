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

        var moduleName = rows[0].SourceModule?.Trim().ToLowerInvariant() ?? string.Empty;
        var text = new StringBuilder();

        switch (moduleName)
        {
            case "receiving":
                text.AppendLine("PO Number\tPart\tDescription\tQty\tWeight\tHeat/Lot\tDate");
                foreach (var row in rows)
                {
                    text.AppendLine($"{row.PONumber ?? string.Empty}\t{row.PartNumber ?? string.Empty}\t{row.PartDescription ?? string.Empty}\t{row.Quantity?.ToString() ?? string.Empty}\t{row.WeightLbs?.ToString() ?? string.Empty}\t{row.HeatLotNumber ?? string.Empty}\t{row.CreatedDate:yyyy-MM-dd}");
                }
                break;

            case "dunnage":
                text.AppendLine("Type\tPart\tSpecs\tQty\tDate");
                foreach (var row in rows)
                {
                    text.AppendLine($"{row.DunnageType ?? string.Empty}\t{row.PartNumber ?? string.Empty}\t{row.SpecsCombined ?? string.Empty}\t{row.Quantity?.ToString() ?? string.Empty}\t{row.CreatedDate:yyyy-MM-dd}");
                }
                break;

            case "volvo":
                text.AppendLine("Shipment #\tPO Number\tReceiver #\tStatus\tPart Count\tDate");
                foreach (var row in rows)
                {
                    text.AppendLine($"{row.ShipmentNumber?.ToString() ?? string.Empty}\t{row.PONumber ?? string.Empty}\t{row.ReceiverNumber ?? string.Empty}\t{row.Status ?? string.Empty}\t{row.PartCount?.ToString() ?? string.Empty}\t{row.CreatedDate:yyyy-MM-dd}");
                }
                break;

            default:
                foreach (var row in rows)
                {
                    text.AppendLine($"{row.SourceModule}\t{row.CreatedDate:yyyy-MM-dd}");
                }
                break;
        }

        return text.ToString();
    }
}