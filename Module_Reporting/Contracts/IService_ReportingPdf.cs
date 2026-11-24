using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Module_Reporting.Contracts;

/// <summary>
/// Renders the report preview into a PDF file the user can keep or attach manually.
/// </summary>
public interface IService_ReportingPdf
{
    /// <summary>
    /// Writes the included preview module cards to <paramref name="filePath"/> as a PDF.
    /// </summary>
    /// <returns>Success with the written file path, or a failure describing the problem.</returns>
    public Task<Model_Dao_Result<string>> CreateReportPdfAsync(
        IReadOnlyList<Model_ReportingPreviewModuleCard> previewModuleCards,
        string documentTitle,
        DateTime generatedAt,
        string generatedBy,
        string filePath,
        CancellationToken cancellationToken = default
    );
}
