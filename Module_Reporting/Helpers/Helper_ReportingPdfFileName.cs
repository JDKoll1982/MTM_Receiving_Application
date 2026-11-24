using System;
using System.IO;
using System.Linq;

namespace MTM_Receiving_Application.Module_Reporting.Helpers;

/// <summary>
/// Builds the default file name offered when the report preview is saved as a PDF.
/// </summary>
public static class Helper_ReportingPdfFileName
{
    private const int MaxBaseNameLength = 90;

    /// <summary>
    /// Builds a file-system-safe PDF file name from the report title, e.g.
    /// <c>End of Day Report for 11-18-2026 - 11-24-2026.pdf</c>.
    /// </summary>
    public static string BuildFileName(string? reportTitle)
    {
        var baseName = string.IsNullOrWhiteSpace(reportTitle)
            ? "Report Summary"
            : reportTitle.Trim();

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(
            baseName
                .Select(character => invalidCharacters.Contains(character) ? '-' : character)
                .ToArray()
        );

        sanitized = string.Join(" ", sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (sanitized.Length == 0)
        {
            sanitized = "Report Summary";
        }

        if (sanitized.Length > MaxBaseNameLength)
        {
            sanitized = sanitized[..MaxBaseNameLength].TrimEnd();
        }

        return $"{sanitized}.pdf";
    }
}
