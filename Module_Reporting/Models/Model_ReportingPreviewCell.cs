using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Reporting.Models;

public class Model_ReportingPreviewCell
{
    public string Value { get; set; } = string.Empty;

    public double Width { get; set; }

    public bool WrapText { get; set; }

    public bool IsNumeric { get; set; }

    public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
}
