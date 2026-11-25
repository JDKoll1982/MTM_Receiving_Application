using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Module_Reporting.Contracts;

/// <summary>
/// Writes the report PDF into its configured output folder and reveals the saved file, so a
/// single Save PDF click both delivers the report and shows the operator where it landed.
/// </summary>
public interface IService_ReportingOutput
{
    /// <summary>
    /// Ensures <paramref name="folderPath"/> exists, then writes the report PDF into it.
    /// </summary>
    /// <param name="previewModuleCards">Report modules to render.</param>
    /// <param name="documentTitle">Title printed in the PDF header.</param>
    /// <param name="folderPath">Destination folder, local or UNC.</param>
    /// <param name="fileName">File name including the .pdf extension.</param>
    /// <param name="generatedAt">Timestamp printed in the PDF footer.</param>
    /// <param name="generatedBy">User printed in the PDF footer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with the full path of the written file.</returns>
    Task<Model_Dao_Result<string>> WriteReportPdfAsync(
        IReadOnlyList<Model_ReportingPreviewModuleCard> previewModuleCards,
        string documentTitle,
        string folderPath,
        string fileName,
        DateTime generatedAt,
        string generatedBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Opens File Explorer at the saved file's folder with that file selected.
    /// </summary>
    /// <param name="filePath">Full path of the file just written.</param>
    Task<Model_Dao_Result> RevealSavedFileAsync(string filePath);
}
