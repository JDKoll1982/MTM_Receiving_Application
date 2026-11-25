using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Module_Reporting.Services;

/// <summary>
/// Owns the file-system side of saving a report PDF: making sure the configured output folder
/// exists, writing the file there, and revealing it in File Explorer.
/// </summary>
public class Service_ReportingOutput : IService_ReportingOutput
{
    private readonly IService_ReportingPdf _reportingPdf;
    private readonly IService_LoggingUtility _logger;

    public Service_ReportingOutput(
        IService_ReportingPdf reportingPdf,
        IService_LoggingUtility logger
    )
    {
        _reportingPdf = reportingPdf ?? throw new ArgumentNullException(nameof(reportingPdf));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<string>> WriteReportPdfAsync(
        IReadOnlyList<Model_ReportingPreviewModuleCard> previewModuleCards,
        string documentTitle,
        string folderPath,
        string fileName,
        DateTime generatedAt,
        string generatedBy,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedFolder = folderPath?.Trim() ?? string.Empty;
        if (normalizedFolder.Length == 0)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                "No report output folder is configured. Set it in Settings > Reporting."
            );
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Model_Dao_Result_Factory.Failure<string>(
                "A report file name could not be determined."
            );
        }

        try
        {
            Directory.CreateDirectory(normalizedFolder);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Could not create the report output folder '{normalizedFolder}': {ex.Message}",
                ex,
                nameof(Service_ReportingOutput)
            );

            return Model_Dao_Result_Factory.Failure<string>(
                $"Could not reach the report folder '{normalizedFolder}'. Check the path in Settings > Reporting. Details: {ex.Message}",
                ex
            );
        }

        var filePath = Path.Combine(normalizedFolder, fileName);

        return await _reportingPdf.CreateReportPdfAsync(
            previewModuleCards,
            documentTitle,
            generatedAt,
            generatedBy,
            filePath,
            cancellationToken
        );
    }

    public Task<Model_Dao_Result> RevealSavedFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure("There is no saved report file to show.")
            );
        }

        try
        {
            // /select opens the containing folder and highlights the file, which answers both
            // "where did it go" and "show me the report".
            Process.Start(
                new ProcessStartInfo("explorer.exe", $"/select,\"{filePath.Trim()}\"")
                {
                    UseShellExecute = true,
                }
            );

            _logger.LogInfo(
                $"Revealed the saved report PDF in File Explorer: {filePath}",
                nameof(Service_ReportingOutput)
            );

            return Task.FromResult(Model_Dao_Result_Factory.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to open File Explorer for the saved report '{filePath}': {ex.Message}",
                ex,
                nameof(Service_ReportingOutput)
            );

            return Task.FromResult(
                Model_Dao_Result_Factory.Failure(
                    $"The report was saved, but File Explorer could not be opened: {ex.Message}",
                    ex
                )
            );
        }
    }
}
