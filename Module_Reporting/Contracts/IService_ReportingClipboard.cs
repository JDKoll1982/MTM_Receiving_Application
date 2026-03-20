using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Reporting.Contracts;

public interface IService_ReportingClipboard
{
    Model_Dao_Result<DataPackage> CreateClipboardPackage(Model_FormattedReportDocument document);
}