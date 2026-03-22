using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Module_Reporting.Contracts;

public interface IService_Reporting
{
    public Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate
    );

    public Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate
    );

    public Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate
    );

    public Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate
    );

    public Task<Model_Dao_Result<List<Model_ReportSummaryTable>>> BuildSummaryTablesAsync(
        List<Model_ReportSection> sections
    );

    public Task<Model_Dao_Result<Model_FormattedReportDocument>> FormatForEmailAsync(
        List<Model_ReportingPreviewModuleCard> previewModuleCards,
        string summaryTitle
    );

    public string NormalizePONumber(string? poNumber);
}
