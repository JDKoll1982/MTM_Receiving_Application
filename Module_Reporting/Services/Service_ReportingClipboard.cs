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
    public Model_Dao_Result<DataPackage> CreateClipboardPackage(Model_FormattedReportDocument document)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentException.ThrowIfNullOrWhiteSpace(document.HtmlFragment);

            var dataPackage = new DataPackage();
            var htmlFormat = HtmlFormatHelper.CreateHtmlFormat(document.HtmlFragment);

            dataPackage.SetHtmlFormat(htmlFormat);
            dataPackage.SetText(document.PlainText);

            return Model_Dao_Result_Factory.Success(dataPackage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<DataPackage>($"Failed to create clipboard package: {ex.Message}", ex);
        }
    }
}