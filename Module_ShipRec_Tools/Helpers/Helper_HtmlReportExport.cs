using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Writes a complete HTML document to a temporary file and opens it in the
/// default browser. Used by tool export flows (Receiving Analytics grid export,
/// Delivery Schedule chart-image export).
/// </summary>
public static class Helper_HtmlReportExport
{
    /// <summary>
    /// Writes <paramref name="html"/> to a temp file named
    /// <c>mtm-{prefix}-{timestamp}.html</c> and opens it with the shell.
    /// </summary>
    public static async Task<Model_Dao_Result<bool>> WriteAndOpenAsync(
        string prefix,
        string html
    )
    {
        try
        {
            var safePrefix = SanitizeFileName(prefix);
            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"mtm-{safePrefix}-{DateTime.Now:yyyyMMdd-HHmmss}.html"
            );

            await File.WriteAllTextAsync(filePath, html, Encoding.UTF8);

            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });

            return Model_Dao_Result_Factory.Success(true);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Failed to open the HTML report: {ex.Message}",
                ex
            );
        }
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "report";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(invalid.Contains(ch) ? '-' : ch);
        }

        var result = builder.ToString().Trim();
        return result.Length == 0 ? "report" : result;
    }
}
