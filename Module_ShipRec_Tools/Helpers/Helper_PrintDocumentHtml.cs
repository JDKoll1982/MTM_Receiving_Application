using System.Net;
using MTM_Receiving_Application.Module_Core.Models.Reporting;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Wraps a formatted report document in a complete HTML page for preview or printing.
/// </summary>
public static class Helper_PrintDocumentHtml
{
    /// <summary>
    /// Builds a complete HTML document around the report's fragment and page CSS.
    /// When <paramref name="autoPrint"/> is true, the page triggers the browser print
    /// dialog on load.
    /// </summary>
    public static string BuildPage(Model_FormattedReportDocument document, bool autoPrint)
    {
        var documentTitle = string.IsNullOrWhiteSpace(document.DocumentTitle)
            ? "Dunnage Book"
            : WebUtility.HtmlEncode(document.DocumentTitle);
        var pageCss = string.IsNullOrWhiteSpace(document.PageCss)
            ? "@page { size: Letter portrait; margin: 0.3in; }"
            : document.PageCss;

        var printScript = autoPrint
            ? """
            <script>
                window.addEventListener('load', function () {
                    window.setTimeout(function () { window.print(); }, 250);
                });
            </script>
"""
            : string.Empty;

        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <title>{{documentTitle}}</title>
    <style>
        {{pageCss}}
        body { margin: 0; background: #ffffff; }
        @media print {
            body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        }
    </style>
    {{printScript}}
</head>
<body>
{{document.HtmlFragment}}
</body>
</html>
""";
    }
}
