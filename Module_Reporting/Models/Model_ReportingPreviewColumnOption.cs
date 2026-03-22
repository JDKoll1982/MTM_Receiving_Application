using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Reporting.Models;

public partial class Model_ReportingPreviewColumnOption : ObservableObject
{
    [ObservableProperty]
    private bool _isIncluded = true;

    public string Key { get; init; } = string.Empty;

    public string Header { get; init; } = string.Empty;

    public double Width { get; init; } = 140d;

    public bool WrapText { get; init; }

    public bool IsNumeric { get; init; }
}